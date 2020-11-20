using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;
using Prism.Windows.Mvvm;
using Prism.Commands;
using Windows.Storage;
using UWP_FileSliceAndMerge_Prism.Services.TxtFile;
using UWP_FileSliceAndMerge_Prism.Helpers;
using UWP_FileSliceAndMerge_Prism.Views;
using System.Diagnostics;

namespace UWP_FileSliceAndMerge_Prism.ViewModels
{
    public class TxtSplitViewModel : ViewModelBase
    {
        private readonly string _settingKey = "TxtSplit_IsSaveOutputFolderAsDefault";
        private readonly string _folderToken = "TxtSplit_OutputFolderToken";
        private readonly int MaxSliceNumber = 999999;

        public ObservableCollection<TxtEntiretyInfoModel> EntiretyFiles { get; set; } =
                            new ObservableCollection<TxtEntiretyInfoModel>();
        private ObservableCollection<TxtSliceInfoModel> _sliceFiles = new ObservableCollection<TxtSliceInfoModel>();
        public ObservableCollection<TxtSliceInfoModel> SliceFiles
        {
            get { return _sliceFiles; }
            set { SetProperty(ref _sliceFiles, value); }
        }

        public List<string> NumberList { get; set; } = new List<string>
        {
            "100",
            "500",
            "1000",
            "5000",
            "10000"
        };

        private int _numberSelected=100;
        private string _numberSelectedText="100";
        public string NumberSelectedText
        {
            get { return _numberSelectedText; }
            set
            {
                if (_numberSelectedText != value)
                {
                    SetProperty(ref _numberSelectedText, value);
                    checkSliceNumber(_numberSelectedText);
                }

            }
        }
        private int _numberUnitSelectedIndex;
        public int NumberUnitSelectedIndex
        {
            get { return _numberUnitSelectedIndex; }
            set
            {
                if (_numberUnitSelectedIndex != value)
                {
                    SetProperty(ref _numberUnitSelectedIndex, value);
                    checkSliceNumber(NumberSelectedText);
                }
            }
        }


        private bool _isStarted = false;
        public bool IsStarted
        {
            get { return _isStarted; }
            set { SetProperty(ref _isStarted, value); }
        }
        private bool _isFinish;
        public bool IsFinish
        {
            get { return _isFinish; }
            set { SetProperty(ref _isFinish, value); }
        }
        private bool _isChinese = true;
        public bool IsChinese
        {
            get { return _isChinese; }
            set { SetProperty(ref _isChinese, value); }
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

        private bool _isSplitByNumber = true;
        public bool IsSplitByNumber
        {
            get { return _isSplitByNumber; }
            set { SetProperty(ref _isSplitByNumber, value); }
        }
        private bool _isSplitNumberWarningShow;
        public bool IsSplitNumberWarningShow
        {
            get { return _isSplitNumberWarningShow; }
            set { SetProperty(ref _isSplitNumberWarningShow, value); }
        }

        public DelegateCommand SelectSourceFilesCommand { get; set; }
        public DelegateCommand ClearSourceFilesCommand { get; set; }
        public DelegateCommand SelectOutputFolderCommand { get; set; }
        public DelegateCommand StartSplitCommand { get; set; }
        public DelegateCommand LaunchFolderCommand { get; set; }

        public TxtSplitViewModel()
        {
            SelectSourceFilesCommand = new DelegateCommand(selectSourceFiles, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            ClearSourceFilesCommand = new DelegateCommand(clearUi, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
            SelectOutputFolderCommand = new DelegateCommand(selectOutputFolder, () => !IsStarted)
                .ObservesProperty(() => IsStarted);
        }

        /// <summary>
        /// 用户选择待处理的源文件
        /// </summary>
        private async void selectSourceFiles()
        {
            var filesPicker = new Windows.Storage.Pickers.FileOpenPicker();
            filesPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filesPicker.FileTypeFilter.Add(".Txt");
            var files = await filesPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                await getSourceFileInfo(files);
                preview();
                //让该Command重新检测是否能够执行的条件
                //StartSplitCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 获取用户选择文件的文件名和大小
        /// </summary>
        /// <returns></returns>
        private async Task getSourceFileInfo(IEnumerable<StorageFile> sourceStorageFiles)
        {
            EntiretyFiles.Clear();
            foreach (StorageFile file in sourceStorageFiles)
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties =
                                            await file.GetBasicPropertiesAsync();
                EntiretyFiles.Add(new TxtEntiretyInfoModel
                {
                    StorageFile = file,
                    FileName = file.Name,
                    FileSize = (long)basicProperties.Size
                });
            }
            await calculateTxtInfo();
        }

        /// <summary>
        /// 清空用户选择的源文件和文件预览列表
        /// </summary>
        private void clearUi()
        {
            IsFinish = false;
            EntiretyFiles.Clear();
            SliceFiles.Clear();
            //让该Command重新检测是否能够执行的条件
            StartSplitCommand.RaiseCanExecuteChanged();
        }

        private async Task calculateTxtInfo()
        {
            TxtInfoService txtInfoService = new TxtInfoService(EntiretyFiles, IsChinese);
            await txtInfoService.FindWordCountAndLineCount();
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

        private void preview()
        {
            SplitPreviewService previewService = new SplitPreviewService(EntiretyFiles, 1, 1);
            List<TxtSliceInfoModel> resultList=previewService.GetPreviewByLineCount(_numberSelected);
            SliceFiles = new ObservableCollection<TxtSliceInfoModel>(resultList);
        }

        /// <summary>
        /// 检测用户输入的切片数量是否合法
        /// </summary>
        private async void checkSliceNumber(string inputText)
        {
            Debug.WriteLine("执行输入检验 " + inputText);
            if (Int32.TryParse(inputText, out int inputNumber) && inputNumber > 0)
            {
                //int sliceNumber = Int32.Parse(inputText);
                if (inputNumber > MaxSliceNumber)
                {
                    await showOverLimitWarning(inputNumber);
                    return;
                }
                _numberSelected = inputNumber;
                preview();
            }
            else
            {
                IsSplitNumberWarningShow = true;
            }
        }

        /// <summary>
        /// 当用户的设定值超过最大切片数量限制，弹出警告。
        /// </summary>
        /// <returns></returns>
        private async Task showOverLimitWarning(long outputFileNumber)
        {
            BinarySplitSettingWarningDialog dialog = new BinarySplitSettingWarningDialog(outputFileNumber);
            await dialog.ShowAsync();
            //Debug.WriteLine("弹出警告窗口");
        }
    }
}
