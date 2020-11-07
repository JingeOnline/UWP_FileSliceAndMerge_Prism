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
using UWP_FileSliceAndMerge_Prism.Services.BinaryFile;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.ViewModels
{
    class BinaryFileSplitViewModel : ViewModelBase
    {
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
            set { SetProperty(ref _sliceNumber, value);previewResultFiles(); }
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
            set { SetProperty(ref _outputFolder, value); }
        }

        public BinaryFileSplitViewModel()
        {
            SelectSourceFilesCommand = new DelegateCommand(selectSourceFiles);
            ClearSourceFilesCommand = new DelegateCommand(clearSourceFiles);
            SelectOutputFolderCommand = new DelegateCommand(selectOutputFolder);
            StartSplitCommand = new DelegateCommand(startSplit);
            SliceNamingRule = SliceNamingRules[0];
            getDefaultFolder();
        }

        private async Task getDefaultFolder()
        {
            var pictureLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            OutputFolder = pictureLibrary.SaveFolder;
            //OutputFolder=await StorageFolder.GetFolderFromPathAsync("C:\\");
            //OutputFolder = await DownloadsFolder.CreateFolderAsync("");
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
                    FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                OutputFolder = folder;
            }
        }

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
        /// 清空用户选择的源文件
        /// </summary>
        private void clearSourceFiles()
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
            PreviewOutput.Clear();

            if (IsSplitBySliceNumber)
            {
                PreviewOutputService previewService =
                    new PreviewOutputService(SliceNamingRule, _indexStartWith);

                //不能直接给ObservableCollection赋新值，会造成UI不更新。
                //如果给ObservableCollection赋新值，需要使用OnPropertyChanged事件，也就是使用SetProperty()方法
                //PreviewOutput =new ObservableCollection<BinarySliceModel>(previewService.GetPreviewSlicesByNumber(inputFiles));

                List<BinarySliceModel> resultList = previewService.GetPreviewSlicesByNumber(SourceFilesInfo, SliceNumber);
                foreach (var result in resultList)
                {
                    PreviewOutput.Add(result);
                }
                
            }
        }

        private async void startSplit()
        {
            BinaryFileSliceAndMergeService sliceService = new BinaryFileSliceAndMergeService(OutputFolder,SourceFiles);
            await sliceService.Slice(PreviewOutput);
            //await sliceService.CopyFile(SourceFiles[0],OutputFolder);
        }
    }
}
