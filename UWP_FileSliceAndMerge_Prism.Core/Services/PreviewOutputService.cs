using System;
using System.Collections.Generic;
using System.Text;
using UWP_FileSliceAndMerge_Prism.Core.Models;
using System.Text.RegularExpressions;


namespace UWP_FileSliceAndMerge_Prism.Core.Services
{
    public class PreviewOutputService
    {
        private int sliceNumber;
        private string namingRule;
        private int indexStartWith;

        public PreviewOutputService(string namingRule, int indexStartWith)
        {
            this.namingRule = namingRule;
            this.indexStartWith = indexStartWith;
        }

        /// <summary>
        /// 根据切片数量来计算名称和大小
        /// </summary>
        /// <param name="sourceFilesInfo"></param>
        /// <returns></returns>
        public List<BinarySliceModel> GetPreviewSlicesByNumber(IEnumerable<BinarySliceModel> sourceFilesInfo,int sliceNumber)
        {
            this.sliceNumber = sliceNumber;
            List<BinarySliceModel> previewSlices = new List<BinarySliceModel>();
            foreach (BinarySliceModel sourceFileInfo in sourceFilesInfo)
            {
                previewSlices.AddRange(getSlicesByNumberFromOneFile(sourceFileInfo));
            }
            return previewSlices;
        }

        /// <summary>
        /// 获得一个源文件的所有切片
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
    }
}
