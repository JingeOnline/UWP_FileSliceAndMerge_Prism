using System;
using System.Collections.Generic;
using System.Text;
using UWP_FileSliceAndMerge_Prism.Core.Models;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace UWP_FileSliceAndMerge_Prism.Core.Services
{
    public class PreviewOutputService
    {
        private int sliceNumber;
        private string namingRule;
        private int indexStartWith;
        private IEnumerable<BinarySliceModel> sourceFilesInfo;
        //private readonly int maxPreviewItemCount = 10000;
        //private int currentPreivewItemCount = 0;

        public PreviewOutputService(string namingRule, int indexStartWith, IEnumerable<BinarySliceModel> sourceFilesInfo)
        {
            this.namingRule = namingRule;
            this.indexStartWith = indexStartWith;
            this.sourceFilesInfo = sourceFilesInfo;
        }

        /// <summary>
        /// 根据切片数量来计算名称和大小
        /// </summary>
        /// <param name="sourceFilesInfo"></param>
        /// <returns></returns>
        public List<BinarySliceModel> GetPreviewSlicesByNumber(int sliceNumber)
        {
            this.sliceNumber = sliceNumber;
            List<BinarySliceModel> previewSlices = new List<BinarySliceModel>();
            foreach (BinarySliceModel sourceFileInfo in sourceFilesInfo)
            {
                previewSlices.AddRange(getSlicesByNumberFromOneFile(sourceFileInfo));
            }
            return previewSlices;
        }

        public List<BinarySliceModel> GetPreviewSlicesBySize(long maxSize)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<BinarySliceModel> previewSlices = new List<BinarySliceModel>();
            foreach(BinarySliceModel sourceFileInfo in sourceFilesInfo)
            {
                previewSlices.AddRange(getSlicesBySizeFromOneFile(sourceFileInfo, maxSize));
            }
            Debug.WriteLine(sw.ElapsedMilliseconds);
            return previewSlices;
        }

        /// <summary>
        /// 获得一个源文件的所有切片（指定切片数）
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        private List<BinarySliceModel> getSlicesByNumberFromOneFile(BinarySliceModel sourceFile)
        {
            List<BinarySliceModel> slices = new List<BinarySliceModel>();
            long avgSlicesSize = sourceFile.FileSize / sliceNumber;
            long lastFileSize = sourceFile.FileSize;
            for (int i = 0; i < sliceNumber-1; i++)
            {
                lastFileSize -= avgSlicesSize;
                slices.Add(new BinarySliceModel()
                {
                    SourceFileName=sourceFile.FileName,
                    FileSize = avgSlicesSize,
                    FileName = getSliceName(sourceFile.FileName,  i + indexStartWith),
                    IsDone=false,
                });
            }
            //最后一个切片的大小和前几个切片大小可能会不同。
            slices.Add(new BinarySliceModel()
            {
                SourceFileName = sourceFile.FileName,
                FileSize = lastFileSize,
                FileName=getSliceName(sourceFile.FileName,sliceNumber-1+indexStartWith),
                IsDone = false,
            });
            return slices;
        }

        /// <summary>
        /// 计算切片文件的文件名
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string getSliceName(string sourceName, int index)
        {
            string[] array = sourceName.Split('.');
            string fileExtention = array[array.Length - 1];
            string fileName = sourceName.Remove(sourceName.Length - fileExtention.Length - 1);
            string name = namingRule;
            if (namingRule.Contains("{*}"))
            {
                name = name.Replace("{*}", fileName);
            }
            if (namingRule.Contains("{#}"))
            {
                name = name.Replace("{#}", index.ToString());
            }

            return name + "." + fileExtention;
        }

        /// <summary>
        /// 获取一个源文件的切片（指定文件大小）
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        private List<BinarySliceModel> getSlicesBySizeFromOneFile(BinarySliceModel sourceFile,long maxSize)
        {
            List<BinarySliceModel> slices = new List<BinarySliceModel>();
            long totalSize = sourceFile.FileSize;
            while (totalSize > maxSize)
            {
                slices.Add(new BinarySliceModel()
                {
                    SourceFileName = sourceFile.FileName,
                    FileSize = maxSize,
                    FileName = getSliceName(sourceFile.FileName, indexStartWith),
                    IsDone = false,
                });
                indexStartWith++;
                totalSize -= maxSize;
            }
            if (totalSize != 0)
            {
                slices.Add(new BinarySliceModel()
                {
                    SourceFileName = sourceFile.FileName,
                    FileSize = totalSize,
                    FileName = getSliceName(sourceFile.FileName, indexStartWith),
                    IsDone = false,
                });
            }
            return slices;
        }
    }
}
