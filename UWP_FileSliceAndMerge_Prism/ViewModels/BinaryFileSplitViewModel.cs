﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Mvvm;
using UWP_FileSliceAndMerge_Prism.Constants;
using UWP_FileSliceAndMerge_Prism.Core.Models;
using UWP_FileSliceAndMerge_Prism.Core.Services;
using UWP_FileSliceAndMerge_Prism.Helpers;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.Services.BinaryFile;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;

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
        public DelegateCommand LaunchFolderCommand { get; set; }

        public List<string> SliceNamingRules { get; set; } = new List<string>()
        {
            "{*}_{#}",
            "{*}-{#}",
            "{*}~{#}",
            "{*}.{#}"
        };
        public List<string> SliceNumberList { get; set; } = new List<string>()
        {
            "2","3","5","10","100",
        };
        public List<string> SliceMaxSizeList { get; set; } = new List<string>()
        {
            "1","2","8","16","128","256","512"
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
        public string SliceNumberText
        {
            get { return _sliceNumber.ToString(); }
            set
            {
                if (checkUserInput(value))
                {
                    SetProperty(ref _sliceNumber, Int32.Parse(value));
                    previewResultFiles();
                }
            }
        }

        private long _sliceMaxSize = 1;
        public string SliceMaxSizeText
        {
            get { return _sliceMaxSize.ToString(); }
            set
            {
                if (checkUserInput(value))
                {
                    SetProperty(ref _sliceMaxSize, long.Parse(value));
                    previewResultFiles();
                }
            }
        }

        private string _sliceMaxSizeUnit = "MB";
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

        private bool _isOutputFolderSetAsDefaultVisiable = false;
        public bool IsOutputFolderSetAsDefaultVisiable
        {
            get { return _isOutputFolderSetAsDefaultVisiable; }
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

        private string _splitMethodWarning;
        public string SplitMethodWarning
        {
            get { return _splitMethodWarning; }
            set
            {
                SetProperty(ref _splitMethodWarning, value);
                if (string.IsNullOrEmpty(value))
                {
                    IsSplitMethodWarningVisible = false;
                }
                else
                {
                    IsSplitMethodWarningVisible = true;
                }
            }
        }

        private string _sliceNamingWarning;
        public string SliceNamingWarning
        {
            get { return _sliceNamingWarning; }
            set
            {
                SetProperty(ref _sliceNamingWarning, value);
                if (string.IsNullOrEmpty(value))
                {
                    IsSliceNamingWarningVisiable = false;
                }
                else
                {
                    IsSliceNamingWarningVisiable = true;
                }
            }
        }

        private bool _isSplitMethodWarningVisible;
        public bool IsSplitMethodWarningVisible
        {
            get { return _isSplitMethodWarningVisible; }
            set { SetProperty(ref _isSplitMethodWarningVisible, value); }
        }

        private bool _isSliceNamingWarningVisiable;
        public bool IsSliceNamingWarningVisiable
        {
            get { return _isSliceNamingWarningVisiable; }
            set { SetProperty(ref _isSliceNamingWarningVisiable, value); }
        }

        private bool _isFinish;
        public bool IsFinish
        {
            get { return _isFinish; }
            set { SetProperty(ref _isFinish, value); }
        }

        private bool _isStarted = false;
        public bool IsStarted
        {
            get { return _isStarted;  }
            set { SetProperty(ref _isStarted, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BinaryFileSplitViewModel()
        {
            //在DelegateCommand中可以写lambda表达式，返回值为bool类型即可。
            //但在ObservesCanExecute和ObservesProperty方法中，看似是lambda表达式，
            //实际上只能传入一个属性，传入其他的值或者表达式会报错。
            SelectSourceFilesCommand = new DelegateCommand(selectSourceFiles,()=>!IsStarted)
                .ObservesProperty(()=> IsStarted);
            ClearSourceFilesCommand = new DelegateCommand(clearUi, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            SelectOutputFolderCommand = new DelegateCommand(selectOutputFolder, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            StartSplitCommand = new DelegateCommand(startSplit,canStart)
                .ObservesProperty(()=>IsFinish).ObservesProperty(()=>IsStarted);
            LaunchFolderCommand = new DelegateCommand(launchFolder).ObservesCanExecute(()=>IsFinish);
            SliceNamingRule = SliceNamingRules[0];
            getAppSetting();
        }

        /// <summary>
        /// 开始按钮是否可用
        /// </summary>
        /// <returns></returns>
        private bool canStart()
        {
            return SourceFiles.Count > 0 && !IsFinish &&!IsStarted;
        }


        /// <summary>
        /// 获取应用程序储存的设置数据
        /// </summary>
        /// <returns></returns>
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
                //让该Command重新检测是否能够执行的条件
                StartSplitCommand.RaiseCanExecuteChanged();
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
            IsFinish = false;
            SourceFiles.Clear();
            SourceFilesInfo.Clear();
            PreviewOutput.Clear();
            //让该Command重新检测是否能够执行的条件
            StartSplitCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 预览处理结果
        /// </summary>
        private void previewResultFiles()
        {

            int maxPreviewItemCount = 5000;
            int currentPreviewItemCount = 0;

            IsFinish = false;
            PreviewOutput.Clear();

            if (IsSplitBySliceNumber)
            {
                PreviewOutputService previewService =
                    new PreviewOutputService(SliceNamingRule, _indexStartWith, SourceFilesInfo);

                //不能直接给ObservableCollection赋新值，会造成UI不更新。
                //如果给ObservableCollection赋新值，需要使用OnPropertyChanged事件，也就是使用SetProperty()方法
                //PreviewOutput =new ObservableCollection<BinarySliceModel>(previewService.GetPreviewSlicesByNumber(inputFiles));

                List<BinarySliceModel> resultList = previewService.GetPreviewSlicesByNumber(_sliceNumber);
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

        /// <summary>
        /// 检测用户输入值是否合法
        /// </summary>
        private bool checkUserInput(string inputText)
        {
            SplitMethodWarning = "";
            if (IsSplitBySliceNumber)
            {
                if (Int32.TryParse(inputText, out int inputNumber) && inputNumber > 0)
                {
                    return true;
                }
                else
                {
                    SplitMethodWarning = BinaryFileErrors.InvalidNumber;
                }
            }
            else
            {
                if (long.TryParse(inputText, out long inputNumber) && inputNumber > 0)
                {
                    return true;
                }
                else
                {
                    SplitMethodWarning = BinaryFileErrors.InvalidNumber;
                }
            }
            return false;
        }

        /// <summary>
        /// 把用户选择的单位转换成Byte
        /// </summary>
        /// <returns></returns>
        private long calculateSliceMaxSize()
        {
            switch (SliceMaxSizeUnit)
            {
                case "Byte":
                    return _sliceMaxSize;
                case "KB":
                    return _sliceMaxSize * 1024;
                case "MB":
                    return _sliceMaxSize * 1024 * 1024;
                case "GB":
                    return _sliceMaxSize * 1024 * 1024 * 1024;
                default:
                    throw new ArgumentException("计算文件大小单位参数异常", $"SliceMaxSizeUnit={SliceMaxSizeUnit}");
            }
        }

        /// <summary>
        /// 开始执行切割
        /// </summary>
        private async void startSplit()
        {
            IsStarted = true;
            BinaryFileSliceAndMergeService sliceService = new BinaryFileSliceAndMergeService(OutputFolder, SourceFiles);
            await sliceService.SplitFiles(PreviewOutput);
            IsFinish = true;
            IsStarted = false;
        }

        /// <summary>
        /// 下载完成后打开文件夹
        /// </summary>
        private async void launchFolder()
        {
            var t = new FolderLauncherOptions();
            await Launcher.LaunchFolderAsync(OutputFolder, t);
        }
    }
}