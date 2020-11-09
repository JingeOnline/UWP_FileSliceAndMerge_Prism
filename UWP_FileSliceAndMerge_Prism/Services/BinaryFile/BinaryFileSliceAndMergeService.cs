using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Core.Models;
using UWP_FileSliceAndMerge_Prism.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;

namespace UWP_FileSliceAndMerge_Prism.Services.BinaryFile
{
    public class BinaryFileSliceAndMergeService
    {
        private int _bufferSize;
        private StorageFolder _outputFolder;
        private IEnumerable<StorageFile> _sourceFiles;

        public BinaryFileSliceAndMergeService(StorageFolder outputFolder,IEnumerable<StorageFile> sourceFiles)
        {
            _outputFolder = outputFolder;
            _sourceFiles = sourceFiles;
            
        }

        /// <summary>
        /// 分隔文件
        /// </summary>
        /// <param name="previewResultCollection"></param>
        /// <returns></returns>
        public async Task SplitFiles(IEnumerable<BinarySliceModel> previewResultCollection)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            setBufferSize(previewResultCollection);
            foreach (StorageFile file in _sourceFiles)
            {
                List<BinarySliceModel> oneFilePreviews = previewResultCollection.Where(x => x.SourceFileName == file.Name).ToList();
                await splitOneFile(file,oneFilePreviews);
            }
            GC.Collect(1);
            GC.Collect(2);
            Debug.WriteLine("当前缓冲区大小 " + _bufferSize);
            Debug.WriteLine("总时长 " + stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// 根据文件大小设置读写缓冲区大小
        /// </summary>
        /// <param name="previewResultCollection"></param>
        private void setBufferSize(IEnumerable<BinarySliceModel> previewResultCollection)
        {
            long previewSliceSize = previewResultCollection.First().FileSize;
            if (previewSliceSize > 1024 * 1024 * 256)
            {
                _bufferSize = 1024 * 1024 * 128;
                return;
            }

            if (previewSliceSize > 1024 * 1024 * 64)
            {
                _bufferSize = 1024 * 1024 * 16;
                return;
            }

            if (previewSliceSize > 1024 * 1024)
            {
                _bufferSize = 1024 * 1024;
                return;
            }

            _bufferSize = 1024*64;
        }

        private async Task splitOneFile(StorageFile sourceFile, List<BinarySliceModel> slicesPreview)
        {
            long streamStartPosition = 0;
            foreach (var preview in slicesPreview)
            {
                await splitOneFileOneSlice(sourceFile, preview, streamStartPosition);
                streamStartPosition += preview.FileSize;
            }
        }

        /// <summary>
        /// 输出一个切片
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="preview"></param>
        /// <param name="streamStartPosition"></param>
        /// <returns></returns>
        private async Task splitOneFileOneSlice(StorageFile sourceFile, BinarySliceModel preview, long streamStartPosition)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            preview.IsStart = true;
            StorageFile targetFile = await _outputFolder.CreateFileAsync(preview.FileName);
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
            preview.IsDone=true;
            preview.FinishSize = preview.FileSize;
            Debug.WriteLine(preview.FileName+"写入用时 "+stopwatch.ElapsedMilliseconds);
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
