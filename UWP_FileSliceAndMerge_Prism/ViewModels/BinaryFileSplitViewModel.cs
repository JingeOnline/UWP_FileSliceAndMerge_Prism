using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Mvvm;
using UWP_FileSliceAndMerge_Prism.Core.Models;
using UWP_FileSliceAndMerge_Prism.Core.Services;
using UWP_FileSliceAndMerge_Prism.Helpers;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.Services.BinaryFile;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace UWP_FileSliceAndMerge_Prism.ViewModels
{
    class BinaryFileSplitViewModel : ViewModelBase
    {
        //应用程序的setting
        private static ApplicationDataContainer _appSetting = ApplicationData.Current.LocalSettings;

        public ObservableCollection<StorageFile> SourceFiles { get; set; } = new ObservableCollection<StorageFile>();
        public List<BinarySliceModel> SourceFilesInfo { get; set; } = new List<BinarySliceModel>();
        public ObservableCollection<BinarySliceModel> PreviewOutput { get; set; } = new ObservableCollection<BinarySliceModel>();
        public DelegateCommand SelectSourceFilesCommand { get; set; }
        public DelegateCommand ClearSourceFilesCommand { get; set; }
        public DelegateCommand SelectOutputFolderCommand { get; set; }
        public DelegateCommand StartSplitCommand { get; set; }

        public List<string> SliceNamingRules { get; set; } = new List<string>()
        {
            "{*}_{#}",
            "{*}-{#}",
            "{*}~{#}",
            "{*}.{#}"
        };
        public List<int> SliceNumberList { get; set; } = new List<int>()
        {
            2,3,5,10,100,
        };
        public List<long> SliceMaxSizeList { get; set; } = new List<long>()
        {
            1,5,10,20,50,100,500
        };
        public List<string> SliceMaxSizeUnitList { get; set; } = new List<string>()
        {
            "Byte","KB","MB","GB"
        };

        private string _slickeNamingRule = "{*}_{#}";
        public string SliceNamingRule
        {
            get { return _slickeNamingRule; }
            set { SetProperty(ref _slickeNamingRule, value); previewResultFiles(); }
        }

        private bool _isSplitBySliceNumber = true;
        public bool IsSplitBySliceNumber
        {
            get { return _isSplitBySliceNumber; }
            set { SetProperty(ref _isSplitBySliceNumber, value); }
        }

        private int _sliceNumber = 2;
        public int SliceNumber
        {
            get { return _sliceNumber; }
            set { SetProperty(ref _sliceNumber, value); previewResultFiles(); }
        }

        private long _sliceMaxSize = 1;
        public long SliceMaxSize
        {
            get { return _sliceMaxSize; }
            set { SetProperty(ref _sliceMaxSize, value);previewResultFiles(); }
        }

        private string _sliceMaxSizeUnit="MB";
        public string SliceMaxSizeUnit
        {
            get { return _sliceMaxSizeUnit; }
            set { SetProperty(ref _sliceMaxSizeUnit, value); previewResultFiles(); }
        }

        private bool _indexStartWith0 = true;
        private int _indexStartWith = 0;
        public bool IndexStartWith0
        {
            get { return _indexStartWith0; }
            set
            {
                SetProperty(ref _indexStartWith0, value);
                _indexStartWith = value ? 0 : 1;
                previewResultFiles();
            }
        }
        private StorageFolder _outputFolder;
        public StorageFolder OutputFolder
        {
            get { return _outputFolder; }
            set
            {
                SetProperty(ref _outputFolder, value);
                if (value != null)
                {
                    IsOutputFolderSetAsDefaultVisiable = true;
                }
            }
        }

        private bool _isOutputFolderSetAsDefaultVisiable=false;
        public bool IsOutputFolderSetAsDefaultVisiable
        {
            get {return _isOutputFolderSetAsDefaultVisiable; }
            set { SetProperty(ref _isOutputFolderSetAsDefaultVisiable, value); }
        }

        private bool _isSaveOutputFolderAsDefault;
        public bool IsSaveOutputFolderAsDefault
        {
            get { return _isSaveOutputFolderAsDefault; }
            set
            {
                SetProperty(ref _isSaveOutputFolderAsDefault, value);
                _appSetting.SaveAsync<bool>("IsSaveOutputFolderAsDefault", value);
            }
        }

        public BinaryFileSplitViewModel()
        {
            
            SelectSourceFilesCommand = new DelegateCommand(selectSourceFiles);
            ClearSourceFilesCommand = new DelegateCommand(clearUi);
            SelectOutputFolderCommand = new DelegateCommand(selectOutputFolder);
            StartSplitCommand = new DelegateCommand(startSplit);
            SliceNamingRule = SliceNamingRules[0];
            getAppSetting();
        }

        private async Task getAppSetting()
        {

            IsSaveOutputFolderAsDefault = await _appSetting.ReadAsync<bool>("IsSaveOutputFolderAsDefault");
            if (IsSaveOutputFolderAsDefault)
            {
                OutputFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("OutputFolderToken");
            }
        }

        /// <summary>
        /// 用户选择待处理的源文件
        /// </summary>
        private async void selectSourceFiles()
        {
            var filesPicker = new Windows.Storage.Pickers.FileOpenPicker();
            filesPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filesPicker.FileTypeFilter.Add("*");
            var files = await filesPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                SourceFiles.Clear();
                foreach (StorageFile file in files)
                {
                    SourceFiles.Add(file);
                }
                await getSourceFileInfo();
                previewResultFiles();
            }
        }

        /// <summary>
        /// 用户选择输出路径
        /// </summary>
        private async void selectOutputFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                //应用此时获得读写该文件夹的权限，包括子文件夹
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                    FutureAccessList.AddOrReplace("OutputFolderToken", folder);
                OutputFolder = folder;
            }
        }

        /// <summary>
        /// 获取用户选择文件的文件名和大小
        /// </summary>
        /// <returns></returns>
        private async Task getSourceFileInfo()
        {
            SourceFilesInfo.Clear();
            foreach (StorageFile file in SourceFiles)
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties =
                                            await file.GetBasicPropertiesAsync();
                SourceFilesInfo.Add(new BinarySliceModel
                {
                    FileName = file.Name,
                    FileSize = (long)basicProperties.Size
                });
            }
        }

        /// <summary>
        /// 清空用户选择的源文件和文件预览列表
        /// </summary>
        private void clearUi()
        {
            SourceFiles.Clear();
            SourceFilesInfo.Clear();
            PreviewOutput.Clear();
            //Debug.WriteLine(IsSplitBySliceNumber);
            //Debug.WriteLine(SliceNumber);
            //Debug.WriteLine(SliceNamingRule);
            //Debug.WriteLine(_indexStartWith);
        }

        /// <summary>
        /// 预览处理结果
        /// </summary>
        private void previewResultFiles()
        {
            int maxPreviewItemCount = 5000;
            int currentPreviewItemCount = 0;

            PreviewOutput.Clear();

            if (IsSplitBySliceNumber)
            {
                PreviewOutputService previewService =
                    new PreviewOutputService(SliceNamingRule, _indexStartWith, SourceFilesInfo);

                //不能直接给ObservableCollection赋新值，会造成UI不更新。
                //如果给ObservableCollection赋新值，需要使用OnPropertyChanged事件，也就是使用SetProperty()方法
                //PreviewOutput =new ObservableCollection<BinarySliceModel>(previewService.GetPreviewSlicesByNumber(inputFiles));

                List<BinarySliceModel> resultList = previewService.GetPreviewSlicesByNumber(SliceNumber);
                foreach (var result in resultList)
                {
                    PreviewOutput.Add(result);
                    currentPreviewItemCount++;
                    if (currentPreviewItemCount >= maxPreviewItemCount)
                    {
                        break;
                    }
                }

            }
            else
            {
                PreviewOutputService previewService =
                    new PreviewOutputService(SliceNamingRule, _indexStartWith, SourceFilesInfo);

                List<BinarySliceModel> resultList = previewService.GetPreviewSlicesBySize(calculateSliceMaxSize());
                foreach (var result in resultList)
                {
                    PreviewOutput.Add(result);
                    currentPreviewItemCount++;
                    if (currentPreviewItemCount >= maxPreviewItemCount)
                    {
                        break;
                    }
                }
            }
        }

        private long calculateSliceMaxSize()
        {
            switch (SliceMaxSizeUnit)
            {
                case "Byte":
                    return SliceMaxSize;
                case "KB":
                    return SliceMaxSize * 1024;
                case "MB":
                    return SliceMaxSize * 1024 * 1024;
                case "GB":
                    return SliceMaxSize * 1024 * 1024 * 1024;
                default:
                    throw new ArgumentException("计算文件大小单位参数异常", $"SliceMaxSizeUnit={SliceMaxSizeUnit}");
            }
        }

        private async void startSplit()
        {
            BinaryFileSliceAndMergeService sliceService = new BinaryFileSliceAndMergeService(OutputFolder, SourceFiles);
            await sliceService.SplitFiles(PreviewOutput);
            //await sliceService.CopyFile(SourceFiles[0],OutputFolder);
        }
    }
}
