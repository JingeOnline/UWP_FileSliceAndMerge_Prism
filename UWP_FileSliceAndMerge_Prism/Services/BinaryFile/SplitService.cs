using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UWP_FileSliceAndMerge_Prism.Services.BinaryFile
{
    public class SplitService
    {
        private int _bufferSize;
        private StorageFolder _outputFolder;
        private IEnumerable<BinaryEntiretyInfoModel> _sourceFiles;

        public SplitService(StorageFolder outputFolder, IEnumerable<BinaryEntiretyInfoModel> sourceFiles)
        {
            _outputFolder = outputFolder;
            _sourceFiles = sourceFiles;

        }

        /// <summary>
        /// 切割文件
        /// </summary>
        /// <param name="previewResultCollection"></param>
        /// <returns></returns>
        public async Task SplitFiles(IEnumerable<BinarySliceInfoModel> previewResultCollection)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //setBufferSize(previewResultCollection);
            foreach (BinaryEntiretyInfoModel file in _sourceFiles)
            {
                List<BinarySliceInfoModel> oneFilePreviews = previewResultCollection.Where(x => x.MergedFileName == file.FileName).ToList();
                await splitOneFile(file, oneFilePreviews);
                file.IsDone = true;
            }
            GC.Collect(1);
            GC.Collect(2);

            Debug.WriteLine($"切割文件总时长 " + stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 根据文件大小设置读写缓冲区大小
        /// </summary>
        /// <param name="sliceSize"></param>
        private void setBufferSize(long sliceSize)
        {
            if (sliceSize >= 1024 * 1024 * 256)
            {
                _bufferSize = 1024 * 1024 * 128;
                return;
            }

            if (sliceSize >= 1024 * 1024 * 64)
            {
                _bufferSize = 1024 * 1024 * 16;
                return;
            }

            if (sliceSize >= 1024 * 1024)
            {
                _bufferSize = 1024 * 1024;
                return;
            }

            _bufferSize = 1024 * 64;
        }

        /// <summary>
        /// 执行一个文件的切割
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="slicesPreview"></param>
        /// <returns></returns>
        private async Task splitOneFile(BinaryEntiretyInfoModel sourceFile, List<BinarySliceInfoModel> slicesPreview)
        {
            sourceFile.IsStart = true;
            setBufferSize(slicesPreview.First().FileSize);
            long streamStartPosition = 0;
            foreach (var preview in slicesPreview)
            {
                await splitOneFileOneSlice(sourceFile.StorageFile, preview, streamStartPosition);
                streamStartPosition += preview.FileSize;
                sourceFile.SliceComplatedNumber++;
                Debug.WriteLine($"切割完成{sourceFile.SliceComplatedNumber}/{sourceFile.SliceNumber}");
            }
        }

        /// <summary>
        /// 输出一个切片
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="preview"></param>
        /// <param name="streamStartPosition"></param>
        /// <returns></returns>
        private async Task splitOneFileOneSlice(StorageFile sourceFile, BinarySliceInfoModel preview, long streamStartPosition)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            preview.IsStart = true;
            StorageFile targetFile = await _outputFolder.CreateFileAsync(preview.FileName, CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine("创建文件用时 " + stopwatch.ElapsedMilliseconds);
            using (Stream readStream = await sourceFile.OpenStreamForReadAsync())
            {
                Debug.WriteLine("创建读取流用时 " + stopwatch.ElapsedMilliseconds);
                readStream.Seek(streamStartPosition, SeekOrigin.Begin);
                Debug.WriteLine("流寻址用时 " + stopwatch.ElapsedMilliseconds);
                long sliceSize = preview.FileSize;
                using (Stream writeStream = await targetFile.OpenStreamForWriteAsync())
                {
                    Debug.WriteLine("创建写入流用时 " + stopwatch.ElapsedMilliseconds);
                    while (true)
                    {
                        byte[] buffer = new byte[_bufferSize];
                        if (sliceSize > _bufferSize)
                        {
                            int n = await readStream.ReadAsync(buffer, 0, buffer.Length);
                            await writeStream.WriteAsync(buffer, 0, n);
                            sliceSize -= _bufferSize;
                        }
                        else
                        {
                            int n = await readStream.ReadAsync(buffer, 0, (int)sliceSize);
                            await writeStream.WriteAsync(buffer, 0, n);
                            break;
                        }
                        //用来更新进度条
                        preview.FinishSize = preview.FileSize - sliceSize;
                        GC.Collect(2);
                    }
                }
            }
            preview.IsDone = true;
            preview.FinishSize = preview.FileSize;
            Debug.WriteLine(preview.FileName + "写入用时 " + stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 测试文件读写
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="targetFolder"></param>
        /// <returns></returns>
        public async Task CopyFile(StorageFile sourceFile, StorageFolder targetFolder)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long currentReadLength = 0;

            StorageFile targetFile = await targetFolder.CreateFileAsync("Copy_" + sourceFile.Name);

            //获取内容流
            using (Stream readStream = await sourceFile.OpenStreamForReadAsync())
            {
                //readStream.Seek();
                using (Stream writeStream = await targetFile.OpenStreamForWriteAsync())
                {
                    while (true)
                    {
                        byte[] buffer = new byte[1024 * 10 * 1024];
                        int n = await readStream.ReadAsync(buffer, 0, buffer.Length);
                        currentReadLength += n;
                        if (n == 0)
                        {
                            break;
                        }
                        await writeStream.WriteAsync(buffer, 0, n);
                        //强制进行二级内存回收（虽然会有开销，但大的缓冲区比小的缓冲区读写效率高很多。特别是针对固态硬盘。）
                        GC.Collect(2);
                    }
                }
            }
            Debug.WriteLine("Length=" + currentReadLength);
            Debug.WriteLine("StopWatch=" + stopwatch.ElapsedMilliseconds);
        }
    }
}
