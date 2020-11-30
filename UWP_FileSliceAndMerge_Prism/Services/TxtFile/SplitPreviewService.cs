using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// 指定行数获得切片预览
        /// </summary>
        /// <param name="lineLimit"></param>
        /// <returns></returns>
        public List<TxtSliceInfoModel> GetPreviewByLineCount(int lineLimit)
        {
            if (_sourceFiles == null || _sourceFiles.Count() == 0)
            {
                return null;
            }
            List<TxtSliceInfoModel> slices = new List<TxtSliceInfoModel>();
            foreach(TxtEntiretyInfoModel sourceFile in _sourceFiles)
            {
                List<TxtSliceInfoModel> oneFileSlices = getPreviewByLineCountForOneSourceFile(sourceFile, lineLimit);
                slices.AddRange(oneFileSlices);
                sourceFile.SliceNumber = oneFileSlices.Count;
            }
            return slices;
        }

        /// <summary>
        /// 指定字数获得切片预览
        /// </summary>
        /// <param name="wordLimit"></param>
        /// <param name="isChinese"></param>
        /// <returns></returns>
        public List<TxtSliceInfoModel> GetPreviewByWordCount(int wordLimit, bool isChinese)
        {
            if (_sourceFiles == null || _sourceFiles.Count() == 0)
            {
                return null;
            }
            List<TxtSliceInfoModel> slices = new List<TxtSliceInfoModel>();
            foreach(TxtEntiretyInfoModel sourceFile in _sourceFiles)
            {
                slices.AddRange(getPreviewByWordCountForOneSourceFile(sourceFile, wordLimit,isChinese));
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
        /// 根据指定字数，获得一个文件的所有切片
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="wordLimit"></param>
        /// <returns></returns>
        private List<TxtSliceInfoModel> getPreviewByWordCountForOneSourceFile
            (TxtEntiretyInfoModel sourceFile, int wordLimit, bool isChinese)
        {
            List<TxtSliceInfoModel> slices = new List<TxtSliceInfoModel>();
            if (isChinese)
            {
                long leftWordCount = sourceFile.TextContent.Length;
                int currentFileIndex = _indexStart;
                int currentWordIndex = 0;
                while (leftWordCount > wordLimit)
                {
                    slices.Add(new TxtSliceInfoModel()
                    {
                        FileName = getSliceName(sourceFile.FileName, currentFileIndex),
                        TextContent = sourceFile.TextContent.Substring(currentWordIndex, wordLimit),
                        Index = (uint)currentFileIndex,
                        MergedFileName = sourceFile.FileName,
                        TxtWordCount = wordLimit,
                    });
                    currentWordIndex += wordLimit;
                    leftWordCount -= wordLimit;
                    currentFileIndex++;
                }
                if (leftWordCount != 0)
                {
                    slices.Add(new TxtSliceInfoModel()
                    {
                        FileName = getSliceName(sourceFile.FileName, currentFileIndex),
                        TextContent = sourceFile.TextContent.Substring(currentWordIndex, (int)leftWordCount),
                        Index = (uint)currentFileIndex,
                        MergedFileName = sourceFile.FileName,
                        TxtWordCount = leftWordCount,
                    });
                }
                return slices;
            }
            else
            {
                //char[] delimiters = new char[] { ' ', '\r', '\n' };
                //string[] words = sourceFile.TextContent.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                //string[] wordsWithLineBreak = sourceFile.TextContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string pattern = @"(?=(?<=^|[^\s])\s+)";
                string[] words = Regex.Split(sourceFile.TextContent, pattern);
                long leftWordCount = words.Length;
                int currentFileIndex = _indexStart;
                int currentWordIndex = 0;
                while (leftWordCount > wordLimit)
                {
                    slices.Add(new TxtSliceInfoModel()
                    {
                        FileName = getSliceName(sourceFile.FileName, currentFileIndex),
                        //TextContent = string.Join(' ',words.Skip(currentWordIndex).Take(wordLimit)),
                        TextContent = string.Join("",words.Skip(currentWordIndex).Take(wordLimit)),
                        Index = (uint)currentFileIndex,
                        MergedFileName = sourceFile.FileName,
                        TxtWordCount = wordLimit,
                    });
                    currentWordIndex += wordLimit;
                    leftWordCount -= wordLimit;
                    currentFileIndex++;
                }
                if (leftWordCount != 0)
                {
                    slices.Add(new TxtSliceInfoModel()
                    {
                        FileName = getSliceName(sourceFile.FileName, currentFileIndex),
                        TextContent = string.Join(' ',words.Skip(currentWordIndex).Take((int)leftWordCount)),
                        Index = (uint)currentFileIndex,
                        MergedFileName = sourceFile.FileName,
                        TxtWordCount = leftWordCount,
                    });
                }
                return slices;
            }
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
