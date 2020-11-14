using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.Services.BinaryFile
{
    public class MergeService
    {
        private int _bufferSize;
        private IEnumerable<BinarySliceInfoModel> _sliceFiles;
        private StorageFolder _outputFolder;

        public MergeService(StorageFolder outputFolder, IEnumerable<BinarySliceInfoModel> sliceFiles)
        {
            _sliceFiles = sliceFiles;
            _outputFolder = outputFolder;
        }

        public async Task MergeFiles(IEnumerable<BinaryEntiretyInfoModel> mergedPreviewFiles)
        {
            //IEnumerable<string> mergedNames = mergedPreviewFiles.Select(x => x.FileName);
            foreach (BinaryEntiretyInfoModel mergedFile in mergedPreviewFiles)
            {
                IEnumerable<BinarySliceInfoModel> slicesForOneFile = _sliceFiles.Where(x => x.MergedFileName == mergedFile.FileName);
                mergedFile.SliceNumber = slicesForOneFile.Count();
                await mergeOneFile(slicesForOneFile, mergedFile);
            }
        }

        private async Task mergeOneFile(IEnumerable<BinarySliceInfoModel> slicesForOneFile, BinaryEntiretyInfoModel mergedFile)
        {
            mergedFile.IsStart = true;
            List<BinarySliceInfoModel> sortedList = slicesForOneFile.OrderBy(x => x.Index).ToList();
            setBufferSize(slicesForOneFile.First().FileSize);
            StorageFile targetFile =
                await _outputFolder.CreateFileAsync(mergedFile.FileName, CreationCollisionOption.ReplaceExisting);
            using (Stream writeStream = await targetFile.OpenStreamForWriteAsync())
            {
                foreach (BinarySliceInfoModel slice in sortedList)
                {
                    slice.IsStart = true;
                    using (Stream readStream = await slice.StorageFile.OpenStreamForReadAsync())
                    {
                        while (true)
                        {
                            byte[] buffer = new byte[_bufferSize];
                            int n = readStream.Read(buffer, 0, _bufferSize);
                            if (n == 0) { break; }
                            writeStream.Write(buffer, 0, n);
                            slice.FinishSize += n;
                            GC.Collect(2);
                        }
                    }
                    slice.IsDone = true;
                    mergedFile.SliceComplatedNumber++;
                    GC.Collect(2);
                }
            }
            mergedFile.IsDone = true;
        }

        /// <summary>
        /// 根据文件大小设置读写缓冲区大小
        /// </summary>
        /// <param name="sliceSize"></param>
        private void setBufferSize(long sliceSize)
        {
            if (sliceSize > 1024 * 1024 * 256)
            {
                _bufferSize = 1024 * 1024 * 128;
                return;
            }

            if (sliceSize > 1024 * 1024 * 64)
            {
                _bufferSize = 1024 * 1024 * 16;
                return;
            }

            if (sliceSize > 1024 * 1024)
            {
                _bufferSize = 1024 * 1024;
                return;
            }

            _bufferSize = 1024 * 64;
        }
    }
}
