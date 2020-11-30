using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.Services.TxtFile
{
    public class SplitService
    {
        private IEnumerable<TxtSliceInfoModel> _slicesPreviews;
        private IEnumerable<TxtEntiretyInfoModel> _sourceFiles;
        private StorageFolder _storageFolder;

        public SplitService(IEnumerable<TxtSliceInfoModel> slicesPreviews,StorageFolder storageFolder,
            IEnumerable<TxtEntiretyInfoModel> sourceFiles)
        {
            _slicesPreviews = slicesPreviews;
            _storageFolder = storageFolder;
            _sourceFiles = sourceFiles;
        }

        public async Task SplitFiles()
        {
            foreach(TxtSliceInfoModel slice in _slicesPreviews)
            {
                StorageFile file=await _storageFolder.CreateFileAsync(slice.FileName, CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(file, slice.TextContent);
                slice.IsDone = true;
                //使用Stream来写入，效果是一样的。
                //var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                //using (var stream = await file.OpenStreamForWriteAsync())
                //{
                //    var bytes = Encoding.UTF8.GetBytes(slice.TextContent);
                //    await stream.WriteAsync(bytes, 0, bytes.Length);
                //}
                TxtEntiretyInfoModel sourceFile = _sourceFiles.First(x => x.FileName == slice.MergedFileName);
                sourceFile.IsStart = true;
                sourceFile.SliceComplatedNumber++;
            }
        }
    }
}
