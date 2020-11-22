using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.Views;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.Services
{
    public static class CheckOutputFileExistingService
    {
        public static async Task<bool> checkOutputFileName(StorageFolder outputFolder, IEnumerable<string> fileNames)
        {
            IReadOnlyList<StorageFile> existedFiles = await outputFolder.GetFilesAsync();
            IEnumerable<string> existedFileNames = existedFiles.Select(x => x.Name);
            //IEnumerable<string> newFileNames = previewFiles.Select(x => x.FileName);
            IEnumerable<string> duplicateNames = existedFileNames.Intersect(fileNames);
            if (duplicateNames.Count() > 0)
            {
                OutputFileNameAlreadyExistedDialog dialog = new OutputFileNameAlreadyExistedDialog(duplicateNames);
                await dialog.ShowAsync();
                if (dialog.AllowReplaceExisting)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
