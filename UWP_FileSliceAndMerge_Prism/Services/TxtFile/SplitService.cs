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
        private StorageFolder _storageFolder;

        public SplitService(IEnumerable<TxtSliceInfoModel> slicesPreviews,StorageFolder storageFolder)
        {
            _slicesPreviews = slicesPreviews;
            _storageFolder = storageFolder;
        }

        public async Task SplitFiles()
        {
            foreach(TxtSliceInfoModel slice in _slicesPreviews)
            {
                StorageFile file=await _storageFolder.CreateFileAsync(slice.FileName, CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(file, slice.TextContent);
                //var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

                //Debug.WriteLine(slice.TextContent.Substring(0, 1000));
                //using (var stream = await file.OpenStreamForWriteAsync())
                //{
                //    var bytes = Encoding.UTF8.GetBytes(slice.TextContent);
                //    await stream.WriteAsync(bytes, 0, bytes.Length);
                //}

            }
        }
    }
}
