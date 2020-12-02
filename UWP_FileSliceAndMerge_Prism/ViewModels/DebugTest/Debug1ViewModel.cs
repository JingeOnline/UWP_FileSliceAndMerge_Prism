using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Mvvm;
using UWP_FileSliceAndMerge_Prism.Models;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.ViewModels
{
    public class Debug1ViewModel:ViewModelBase
    {
        public ObservableCollection<TxtEntiretyInfoModel> EntiretyFiles { get; set; } =
                            new ObservableCollection<TxtEntiretyInfoModel>();
        public DelegateCommand SelectSourceFilesCommand { get; set; }
        public DelegateCommand ClearSourceFilesCommand { get; set; }

        public Debug1ViewModel()
        {
            SelectSourceFilesCommand = new DelegateCommand(async () => await selectSourceFiles());
            ClearSourceFilesCommand = new DelegateCommand(clearUi);
        }

        /// <summary>
        /// 用户选择待处理的源文件
        /// </summary>
        private async Task selectSourceFiles()
        {
            var filesPicker = new Windows.Storage.Pickers.FileOpenPicker();
            filesPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filesPicker.FileTypeFilter.Add(".Txt");
            var files = await filesPicker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                await getSourceFileInfo(files);
            }
        }

        /// <summary>
        /// 获取用户选择文件的文件名和大小
        /// </summary>
        /// <returns></returns>
        private async Task getSourceFileInfo(IEnumerable<StorageFile> sourceStorageFiles)
        {
            foreach (StorageFile file in sourceStorageFiles)
            {
                Windows.Storage.FileProperties.BasicProperties basicProperties =
                                            await file.GetBasicPropertiesAsync();
                EntiretyFiles.Add(new TxtEntiretyInfoModel
                {
                    StorageFile = file,
                    FileName = file.Name,
                    FileSize = (long)basicProperties.Size,
                    TxtWordCount = 100,
                    TxtLineCount = 100,
                    TextLength = 100
                });
            }
        }

        /// <summary>
        /// 清空用户选择的源文件和文件预览列表
        /// </summary>
        private void clearUi()
        {
            EntiretyFiles.Clear();
        }
    }
}
