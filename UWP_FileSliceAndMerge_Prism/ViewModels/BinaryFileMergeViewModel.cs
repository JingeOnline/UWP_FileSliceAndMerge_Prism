using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Mvvm;
using UWP_FileSliceAndMerge_Prism.Helpers;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.Services.BinaryFile;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace UWP_FileSliceAndMerge_Prism.ViewModels
{
    class BinaryFileMergeViewModel : ViewModelBase
    {
        private readonly string _settingKey = "BinaryMerge_IsSaveOutputFolderAsDefault";
        private readonly string _folderToken = "BinaryMerge_OutputFolderToken";

        public DelegateCommand SelectSourceFilesCommand { get; set; }
        public DelegateCommand ClearSourceFilesCommand { get; set; }
        public DelegateCommand SelectOutputFolderCommand { get; set; }
        public DelegateCommand StartMergeCommand { get; set; }
        public DelegateCommand LaunchFolderCommand { get; set; }
        private ObservableCollection<BinaryEntiretyInfoModel> _mergedFiles;
        public ObservableCollection<BinaryEntiretyInfoModel> MergedFiles
        {
            get { return _mergedFiles; }
            set { SetProperty(ref _mergedFiles, value); }
        }
        private ObservableCollection<BinarySliceInfoModel> _sliceFiles = new ObservableCollection<BinarySliceInfoModel>();
        public ObservableCollection<BinarySliceInfoModel> SliceFiles
        {
            get { return _sliceFiles; }
            set { SetProperty(ref _sliceFiles, value); }
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

        public BinaryFileMergeViewModel()
        {
            SelectSourceFilesCommand = new DelegateCommand(selectSourceFiles);
            ClearSourceFilesCommand = new DelegateCommand(clearUi, () => !IsStarted)
                                        .ObservesProperty(() => IsStarted);
            SelectOutputFolderCommand = new DelegateCommand(selectOutputFolder, () => !IsStarted)
                                        .ObservesProperty(() => IsStarted);
            getAppSetting();
        }


        /// <summary>
        /// 用户选择要合并的文件
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
                //StartSplitCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 获取用户选择文件的文件名和大小
        /// </summary>
        /// <returns></returns>
        private async Task getSourceFileInfo(IEnumerable<StorageFile> sourceStorageFiles)
        {
            SliceFiles.Clear();
            foreach (StorageFile file in sourceStorageFiles)
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties =
                                            await file.GetBasicPropertiesAsync();
                SliceFiles.Add(new BinarySliceInfoModel
                {
                    StorageFile = file,
                    FileName = file.Name,
                    FileSize = (long)basicProperties.Size
                });
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
        /// 预览输出结果
        /// </summary>
        private void preview()
        {
            MergePreviewService previewService = new MergePreviewService(SliceFiles);
            MergedFiles = new ObservableCollection<BinaryEntiretyInfoModel>(previewService.GetPreview());
        }

        /// <summary>
        /// 清空用户选择的源文件和文件预览列表
        /// </summary>
        private void clearUi()
        {
            IsFinish = false;
            SliceFiles.Clear();
            MergedFiles.Clear();
            //让该Command重新检测是否能够执行的条件
            StartMergeCommand.RaiseCanExecuteChanged();
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
    }
}
