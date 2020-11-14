using System;
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
using UWP_FileSliceAndMerge_Prism.Helpers;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.Services;
using UWP_FileSliceAndMerge_Prism.Services.BinaryFile;
using UWP_FileSliceAndMerge_Prism.Views;
using Windows.ApplicationModel.Store.Preview.InstallControl;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Core;

namespace UWP_FileSliceAndMerge_Prism.ViewModels
{
    class BinaryFileSplitViewModel : ViewModelBase
    {
        private readonly string _settingKey = "BinaryMerge_IsSaveOutputFolderAsDefault";
        private readonly string _folderToken = "BinaryMerge_OutputFolderToken";
        private readonly int MaxSliceNumber = 999999;

        public ObservableCollection<BinaryEntiretyInfoModel> MergedFiles { get; set; } =
            new ObservableCollection<BinaryEntiretyInfoModel>();

        private ObservableCollection<BinarySliceInfoModel> _sliceFiles = new ObservableCollection<BinarySliceInfoModel>();
        public ObservableCollection<BinarySliceInfoModel> SliceFiles
        {
            get { return _sliceFiles; }
            set { SetProperty(ref _sliceFiles, value); }
        }
        public DelegateCommand SelectSourceFilesCommand { get; set; }
        public DelegateCommand ClearSourceFilesCommand { get; set; }
        public DelegateCommand SelectOutputFolderCommand { get; set; }
        public DelegateCommand StartSplitCommand { get; set; }
        public DelegateCommand LaunchFolderCommand { get; set; }

        public List<string> SliceNamingRules { get; set; } = new List<string>()
        {
            "{@}_{#}",
            "{@}-{#}",
            "{@} ({#})"
        };
        public List<string> SliceNumberList { get; set; } = new List<string>()
        {
            "2","3","5","10","100",
        };
        public List<string> SliceMaxSizeList { get; set; } = new List<string>()
        {
            "1","2","8","32","128","512"
        };
        public List<string> SliceMaxSizeUnitList { get; set; } = new List<string>()
        {
            "Byte","KB","MB","GB"
        };

        private string _sliceNamingRule = "{@}_{#}";
        public string SliceNamingRule
        {
            get { return _sliceNamingRule; }
            set
            {
                if (_sliceNamingRule != value)
                {
                    SetProperty(ref _sliceNamingRule, value);
                    if (checkFileName(value))
                    {
                        preview();
                    }
                }
            }
        }

        private string _sliceFileExtention;
        public string SliceFileExtention
        {
            get { return _sliceFileExtention; }
            set
            {
                if (_sliceFileExtention != value)
                {
                    if (checkFileName(value))
                    {
                        SetProperty(ref _sliceFileExtention, value);
                        preview();
                    }
                }
            }
        }

        private bool _isCustomizeExtention = false;
        public bool IsCustomizeExtention
        {
            get { return _isCustomizeExtention; }
            set
            {
                if (_isCustomizeExtention != value)
                {
                    SetProperty(ref _isCustomizeExtention, value);
                    preview();
                }
            }
        }

        private bool _isSplitBySliceNumber = true;
        public bool IsSplitBySliceNumber
        {
            get { return _isSplitBySliceNumber; }
            set
            {
                SetProperty(ref _isSplitBySliceNumber, value);
                if (value)
                {
                    checkSliceNumber(_sliceNumber.ToString());
                }
                else
                {
                    checkSliceMaxSize(SliceMaxSizeText);
                }
            }
        }

        private int _sliceNumber = 2;
        private string _sliceNumberText = "2";
        public string SliceNumberText
        {
            get { return _sliceNumber.ToString(); }
            set
            {
                if (_sliceNumberText != value)
                {
                    _sliceNumberText = value;
                    checkSliceNumber(value);
                }
            }
        }

        private long _sliceMaxSize = 1;
        private long _sliceMaxSizeTextNumber = 1;
        private string _sliceMaxSizeText = "1";
        public string SliceMaxSizeText
        {
            get { return _sliceMaxSizeText; }
            set
            {
                if (_sliceMaxSizeText != value)
                {
                    _sliceMaxSizeText = value;
                    checkSliceMaxSize(value);
                }
            }
        }

        private string _sliceMaxSizeUnit = "MB";
        public string SliceMaxSizeUnit
        {
            get { return _sliceMaxSizeUnit; }
            set
            {
                if (value != _sliceMaxSizeUnit)
                {
                    SetProperty(ref _sliceMaxSizeUnit, value);
                    checkSliceMaxSizeUnit();
                    //Debug.WriteLine("_sliceMaxSizeUnit=" + _sliceMaxSizeUnit);
                }
            }
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
                preview();
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
                MyAppSettingHelper.AppSetting.SaveAsync<bool>(_settingKey, value);
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
            get { return _isStarted; }
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
            SelectSourceFilesCommand = new DelegateCommand(selectSourceFiles, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            ClearSourceFilesCommand = new DelegateCommand(clearUi, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            SelectOutputFolderCommand = new DelegateCommand(selectOutputFolder, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            StartSplitCommand = new DelegateCommand(startSplit, canStart)
                .ObservesProperty(() => IsFinish).ObservesProperty(() => IsStarted);
            LaunchFolderCommand = new DelegateCommand(launchFolder).ObservesCanExecute(() => IsFinish);
            SliceNamingRule = SliceNamingRules[0];
            getAppSetting();
        }

        /// <summary>
        /// 开始按钮是否可用
        /// </summary>
        /// <returns></returns>
        private bool canStart()
        {
            return MergedFiles.Count > 0 && !IsFinish && !IsStarted;
        }


        /// <summary>
        /// 获取应用程序储存的设置数据
        /// </summary>
        /// <returns></returns>
        private async Task getAppSetting()
        {

            IsSaveOutputFolderAsDefault = await MyAppSettingHelper.AppSetting.ReadAsync<bool>(_settingKey);
            if (IsSaveOutputFolderAsDefault)
            {
                OutputFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(_folderToken);
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
                await getSourceFileInfo(files);
                preview();
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
                    FutureAccessList.AddOrReplace(_folderToken, folder);
                OutputFolder = folder;
            }
        }

        /// <summary>
        /// 获取用户选择文件的文件名和大小
        /// </summary>
        /// <returns></returns>
        private async Task getSourceFileInfo(IEnumerable<StorageFile> sourceStorageFiles)
        {
            MergedFiles.Clear();
            foreach (StorageFile file in sourceStorageFiles)
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties =
                                            await file.GetBasicPropertiesAsync();
                MergedFiles.Add(new BinaryEntiretyInfoModel
                {
                    StorageFile=file,
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
            MergedFiles.Clear();
            SliceFiles.Clear();
            //让该Command重新检测是否能够执行的条件
            StartSplitCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 预览处理结果
        /// </summary>
        private void preview()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (MergedFiles.Count == 0) { return; }
            IsFinish = false;
            SliceFiles.Clear();
            //SliceFiles.Clear();

            SplitPreviewService previewService;
            if (!IsCustomizeExtention)
            {
                previewService = new SplitPreviewService(SliceNamingRule, _indexStartWith, MergedFiles);
            }
            else
            {
                previewService = new SplitPreviewService(SliceNamingRule, SliceFileExtention, _indexStartWith, MergedFiles);
            }


            List<BinarySliceInfoModel> resultList;
            if (IsSplitBySliceNumber)
            {
                //不能直接给ObservableCollection赋新值，会造成UI不更新。
                //如果给ObservableCollection赋新值，需要使用OnPropertyChanged事件，也就是使用SetProperty()方法
                //PreviewOutput =new ObservableCollection<BinarySliceModel>(previewService.GetPreviewSlicesByNumber(inputFiles));
                resultList = previewService.GetPreviewSlicesByNumber(_sliceNumber);
            }
            else
            {
                resultList = previewService.GetPreviewSlicesBySize(_sliceMaxSize);
            }
            Debug.WriteLine("预览计算完成 " + sw.ElapsedMilliseconds);
            //SliceFiles = resultList;
            SliceFiles = new ObservableCollection<BinarySliceInfoModel>(resultList);
            Debug.WriteLine("预览赋值完成 " + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// 检测用户输入的切片数量是否合法
        /// </summary>
        private async void checkSliceNumber(string inputText)
        {
            SplitMethodWarning = "";
            if (Int32.TryParse(inputText, out int inputNumber) && inputNumber > 0)
            {
                //int sliceNumber = Int32.Parse(inputText);
                if (inputNumber > MaxSliceNumber)
                {
                    await showOverLimitWarning(inputNumber);
                    return;
                }
                _sliceNumber = inputNumber;
                preview();
            }
            else
            {
                SplitMethodWarning = BinaryFileErrors.InvalidNumber;
            }
        }

        /// <summary>
        /// 检测用户输入的切片大小是否合法
        /// </summary>
        /// <param name="inputText"></param>
        private async void checkSliceMaxSize(string inputText)
        {
            SplitMethodWarning = "";
            if (long.TryParse(inputText, out long inputNumber) && inputNumber > 0)
            {
                _sliceMaxSizeTextNumber = inputNumber;
                //if(inputNumber)
                long sliceCount = 0;
                foreach (BinaryEntiretyInfoModel sourceFile in MergedFiles)
                {
                    sliceCount += sourceFile.FileSize / calculateSliceMaxSize(inputNumber);
                }
                if (sliceCount > MaxSliceNumber)
                {
                    await showOverLimitWarning(sliceCount);
                    return;
                }
                _sliceMaxSize = calculateSliceMaxSize(inputNumber);
                preview();
            }
            else
            {
                SplitMethodWarning = BinaryFileErrors.InvalidNumber;
            }
        }

        /// <summary>
        /// 检测用户选择的切片大小单位是否合法
        /// </summary>
        /// <param name="unit"></param>
        private async void checkSliceMaxSizeUnit()
        {
            SplitMethodWarning = "";
            long sliceCount = 0;
            foreach (BinaryEntiretyInfoModel file in MergedFiles)
            {
                sliceCount += file.FileSize / calculateSliceMaxSize(_sliceMaxSizeTextNumber);
            }
            if (sliceCount > MaxSliceNumber)
            {
                await showOverLimitWarning(sliceCount);
                return;
            }
            _sliceMaxSize = calculateSliceMaxSize(_sliceMaxSizeTextNumber);
            preview();
        }

        /// <summary>
        /// 当用户的设定值超过最大切片数量限制，弹出警告。
        /// </summary>
        /// <returns></returns>
        private async Task showOverLimitWarning(long outputFileNumber)
        {
            BinarySplitSettingWarningDialog dialog = new BinarySplitSettingWarningDialog(outputFileNumber);
            await dialog.ShowAsync();
        }

        /// <summary>
        /// 把用户选择的单位转换成Byte
        /// </summary>
        /// <returns></returns>
        private long calculateSliceMaxSize(long sliceSizeNumber)
        {
            switch (SliceMaxSizeUnit)
            {
                case "Byte":
                    return sliceSizeNumber;
                case "KB":
                    return sliceSizeNumber * 1024;
                case "MB":
                    return sliceSizeNumber * 1024 * 1024;
                case "GB":
                    return sliceSizeNumber * 1024 * 1024 * 1024;
                default:
                    throw new ArgumentException("计算文件大小单位参数异常", $"sliceSizeNumber={sliceSizeNumber},unit={SliceMaxSizeUnit}");
            }
        }

        /// <summary>
        /// 开始执行切割
        /// </summary>
        private async void startSplit()
        {
            if (!await CheckOutputFileExistingService.checkOutputFileName(OutputFolder, SliceFiles))
            {
                return;
            }
            IsStarted = true;
            SplitService sliceService = new SplitService(OutputFolder, MergedFiles);
            await sliceService.SplitFiles(SliceFiles);
            IsFinish = true;
            IsStarted = false;
            new ToastNotificationsService().ShowTaskFinishToast("Split Complete",
                $"Successfully exported {SliceFiles.Count} slice files.");
        }

        /// <summary>
        /// 下载完成后打开文件夹
        /// </summary>
        private async void launchFolder()
        {
            var t = new FolderLauncherOptions();
            await Launcher.LaunchFolderAsync(OutputFolder, t);
        }

        /// <summary>
        /// 检查用户输入的文件名是否是合法的windows文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool checkFileName(string fileName)
        {
            bool isValid = FileNameCheck.Check(fileName);
            if (!isValid)
            {
                SliceNamingWarning = BinaryFileErrors.InvalidFileName;
                IsSliceNamingWarningVisiable = true;
            }
            else
            {
                IsSliceNamingWarningVisiable = false;
            }
            return isValid;
        }

    }
}
