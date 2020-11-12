using System;
using System.Collections.Generic;
using System.Text;
using UWP_FileSliceAndMerge_Prism.Models;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace UWP_FileSliceAndMerge_Prism.Services.BinaryFile
{
    public class PreviewOutputService
    {
        private int _sliceNumber;
        private string _namingRule;
        private string _fileExtention;
        private int _indexStartWith;
        private IEnumerable<FileInfoModel> _sourceFilesInfo;

        public PreviewOutputService(string namingRule, int indexStartWith, IEnumerable<FileInfoModel> sourceFilesInfo)
        {
            this._namingRule = namingRule;
            this._indexStartWith = indexStartWith;
            this._sourceFilesInfo = sourceFilesInfo;
        }
        public PreviewOutputService(string namingRule,string fileExtention, int indexStartWith, IEnumerable<FileInfoModel> sourceFilesInfo)
        {
            this._namingRule = namingRule;
            this._fileExtention = fileExtention;
            this._indexStartWith = indexStartWith;
            this._sourceFilesInfo = sourceFilesInfo;
        }


        /// <summary>
        /// 根据切片数量来计算名称和大小
        /// </summary>
        /// <param name="sourceFilesInfo"></param>
        /// <returns></returns>
        public List<FileInfoModel> GetPreviewSlicesByNumber(int sliceNumber)
        {
            this._sliceNumber = sliceNumber;
            List<FileInfoModel> previewSlices = new List<FileInfoModel>();
            foreach (FileInfoModel sourceFileInfo in _sourceFilesInfo)
            {
                previewSlices.AddRange(getSlicesByNumberFromOneFile(sourceFileInfo));
            }
            return previewSlices;
        }

        public List<FileInfoModel> GetPreviewSlicesBySize(long maxSize)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            List<FileInfoModel> previewSlices = new List<FileInfoModel>();
            foreach(FileInfoModel sourceFileInfo in _sourceFilesInfo)
            {
                previewSlices.AddRange(getSlicesBySizeFromOneFile(sourceFileInfo, maxSize));
            }
            //Debug.WriteLine(sw.ElapsedMilliseconds);
            return previewSlices;
        }

        /// <summary>
        /// 获得一个源文件的所有切片（指定切片数）
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        private List<FileInfoModel> getSlicesByNumberFromOneFile(FileInfoModel sourceFile)
        {
            List<FileInfoModel> slices = new List<FileInfoModel>();
            long avgSlicesSize = sourceFile.FileSize / _sliceNumber;
            long lastFileSize = sourceFile.FileSize;
            for (int i = 0; i < _sliceNumber-1; i++)
            {
                lastFileSize -= avgSlicesSize;
                slices.Add(new FileInfoModel()
                {
                    SourceFileName=sourceFile.FileName,
                    FileSize = avgSlicesSize,
                    FileName = getSliceName(sourceFile.FileName,  i + _indexStartWith),
                    IsDone=false,
                });
            }
            //最后一个切片的大小和前几个切片大小可能会不同。
            slices.Add(new FileInfoModel()
            {
                SourceFileName = sourceFile.FileName,
                FileSize = lastFileSize,
                FileName=getSliceName(sourceFile.FileName,_sliceNumber-1+_indexStartWith),
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
            string name = _namingRule;
            if (_namingRule.Contains("{@}"))
            {
                name = name.Replace("{@}", fileName);
            }
            if (_namingRule.Contains("{#}"))
            {
                name = name.Replace("{#}", index.ToString());
            }

            if (string.IsNullOrEmpty(_fileExtention))
            {
                return name + "." + fileExtention;
            }
            else
            {
                return name + "." + _fileExtention;
            }
            
        }

        /// <summary>
        /// 获取一个源文件的切片（指定文件大小）
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="maxSize"></param>
        /// <returns></returns>
        private List<FileInfoModel> getSlicesBySizeFromOneFile(FileInfoModel sourceFile,long maxSize)
        {
            List<FileInfoModel> slices = new List<FileInfoModel>();
            long totalSize = sourceFile.FileSize;
            while (totalSize > maxSize)
            {
                slices.Add(new FileInfoModel()
                {
                    SourceFileName = sourceFile.FileName,
                    FileSize = maxSize,
                    FileName = getSliceName(sourceFile.FileName, _indexStartWith),
                    IsDone = false,
                });
                _indexStartWith++;
                totalSize -= maxSize;
            }
            if (totalSize != 0)
            {
                slices.Add(new FileInfoModel()
                {
                    SourceFileName = sourceFile.FileName,
                    FileSize = totalSize,
                    FileName = getSliceName(sourceFile.FileName, _indexStartWith),
                    IsDone = false,
                });
            }
            return slices;
        }
    }
}
