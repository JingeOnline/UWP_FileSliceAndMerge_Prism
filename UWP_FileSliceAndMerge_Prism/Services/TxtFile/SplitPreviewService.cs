using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;

namespace UWP_FileSliceAndMerge_Prism.Services.TxtFile
{
    public class SplitPreviewService
    {
        private IEnumerable<TxtEntiretyInfoModel> _sourceFiles;
        private int _indexStart;
        private int _indexRule;

        public SplitPreviewService(IEnumerable<TxtEntiretyInfoModel> sourceFiles, int indexStart, int indexRule)
        {
            _sourceFiles = sourceFiles;
            _indexStart = indexStart;
            _indexRule = indexRule;
        }

        public List<TxtSliceInfoModel> GetPreviewByLineCount(int lineLimit)
        {
            if (_sourceFiles == null || _sourceFiles.Count() == 0)
            {
                return null;
            }
            List<TxtSliceInfoModel> slices = new List<TxtSliceInfoModel>();
            foreach(TxtEntiretyInfoModel sourceFile in _sourceFiles)
            {
                slices.AddRange(getPreviewByLineCountForOneSourceFile(sourceFile,lineLimit));
            }
            return slices;
        }

        /// <summary>
        /// 根据指定行数，获得一个文件的所有切片
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="lineLimit"></param>
        /// <returns></returns>
        private  List<TxtSliceInfoModel> getPreviewByLineCountForOneSourceFile
            (TxtEntiretyInfoModel sourceFile, int lineLimit)
        {
            List<TxtSliceInfoModel> slices = new List<TxtSliceInfoModel>();
            //string content = await Windows.Storage.FileIO.ReadTextAsync(sourceFile.StorageFile);
            string content = sourceFile.TextContent;
            string[] lines = content.Split('\n');
            long leftLinesNum = lines.Length;
            int currentLineIndex = 0;
            int currentFileIndex = _indexStart;
            while (leftLinesNum > lineLimit)
            {
                IEnumerable<string> sliceLines = lines.Skip(currentLineIndex).Take(lineLimit);
                slices.Add(new TxtSliceInfoModel()
                {
                    FileName = getSliceName(sourceFile.FileName, currentFileIndex),
                    TextContent = string.Join('\n', sliceLines),
                    Index = (uint)currentFileIndex,
                    MergedFileName = sourceFile.FileName,
                    TxtLineCount = lineLimit,
                });
                currentLineIndex += lineLimit;
                leftLinesNum -= lineLimit;
                currentFileIndex++;
            }
            //最后一个文件
            if (leftLinesNum != 0)
            {
                IEnumerable<string> sliceLines = lines.Skip(currentLineIndex);
                slices.Add(new TxtSliceInfoModel()
                {
                    FileName = getSliceName(sourceFile.FileName, currentFileIndex),
                    TextContent = string.Join('\n', sliceLines),
                    Index = (uint)currentFileIndex,
                    MergedFileName = sourceFile.FileName,
                    TxtLineCount = sliceLines.Count(),
                });
            }
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
            string name = null;
            switch (_indexRule)
            {
                case 0: name = fileName + "_" + index; break;
                case 1: name = fileName + "-" + index; break;
                case 2: name = fileName + "(" + index + ")"; break;
                case 3: name = fileName + " (" + index + ")"; break;
            }
            return name + "." + fileExtention;


        }
    }
}
