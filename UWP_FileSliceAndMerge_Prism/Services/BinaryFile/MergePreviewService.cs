using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_FileSliceAndMerge_Prism.Models;
using System.Text.RegularExpressions;

namespace UWP_FileSliceAndMerge_Prism.Services.BinaryFile
{
    public class MergePreviewService
    {
        private string _givenFileExtention;
        private IEnumerable<BinarySliceInfoModel> _sourceFiles;
        private List<BinaryEntiretyInfoModel> _mergedFiles;

        public MergePreviewService(IEnumerable<BinarySliceInfoModel> sourceFiles, string givenFileExtention = null)
        {
            _sourceFiles = sourceFiles;
            _givenFileExtention = givenFileExtention;
        }

        /// <summary>
        /// 返回合并后的结果列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BinaryEntiretyInfoModel> GetPreview()
        {
            if (_sourceFiles.Count() == 0) { return null; }
            calculateMergedNames();
            mergeSizePreview();
            return _mergedFiles;
        }

        /// <summary>
        /// 获得每个切片的序号和对应的原始文件名
        /// </summary>
        private void calculateMergedNames()
        {
            //Dictionary<string, BinarySliceInfoModel> fileNameWithoutExtMatchToSlice = new Dictionary<string, BinarySliceInfoModel>();
            foreach (BinarySliceInfoModel slice in _sourceFiles)
            {
                //去除扩展名
                string[] array = slice.FileName.Split('.');
                string fileExtention = array[array.Length - 1];
                string fileNameWithoutExtention = slice.FileName.Remove(slice.FileName.Length - fileExtention.Length - 1);
                //fileNameWithoutExtMatchToSlice.Add(fileNameWithoutExtention, slice);

                findIndex(slice, fileNameWithoutExtention);
                findSourceFile(slice, fileNameWithoutExtention, fileExtention);
            }
        }

        /// <summary>
        /// 查找切片的序号
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="nameWithoutExtention"></param>
        private void findIndex(BinarySliceInfoModel slice, string nameWithoutExtention)
        {
            string pattern = "[\\d]+";

            //从右开始查找第一组数字
            Match match = Regex.Match(nameWithoutExtention, pattern, RegexOptions.RightToLeft);
            if (uint.TryParse(match.Value, out uint index))
            {
                slice.Index = index;
            }
        }

        /// <summary>
        /// 查找切片的原始文件名
        /// </summary>
        /// <param name="slice"></param>
        /// <param name="nameWithoutExtention"></param>
        private void findSourceFile(BinarySliceInfoModel slice, string nameWithoutExtention, string sourceExtention)
        {
            string pattern = "[_|\\-|\\(| ]*[\\d]+[\\)]*";

            //使用参数RightToLeft，可以让查找从字符串的最后开始。
            //从右查找出-123,_123,(123),-((123))这样字符串，并替换为空。
            Match match = Regex.Match(nameWithoutExtention, pattern, RegexOptions.RightToLeft);
            //string sourceNameWithoutExtention = Regex.Replace(nameWithoutExtention, pattern, "", RegexOptions.RightToLeft);
            string sourceNameWithoutExtention;
            if (match.Success)
            {
                int i = nameWithoutExtention.LastIndexOf(match.Value);
                sourceNameWithoutExtention = nameWithoutExtention.Substring(0, i);
            }
            else
            {
                sourceNameWithoutExtention = nameWithoutExtention;
            }

            if (string.IsNullOrEmpty(_givenFileExtention))
            {
                slice.MergedFileName = sourceNameWithoutExtention + "." + sourceExtention;
            }
            else
            {
                slice.MergedFileName = sourceNameWithoutExtention + "." + _givenFileExtention;
            }
        }

        /// <summary>
        /// 创建每一个源文件对象并添加到List中。
        /// </summary>
        private void mergeSizePreview()
        {
            _mergedFiles = new List<BinaryEntiretyInfoModel>();
            List<string> sourceFileNames =
                _sourceFiles.Select(file => file.MergedFileName).Distinct().ToList();
            foreach (string sourceName in sourceFileNames)
            {
                BinaryEntiretyInfoModel sourceFile = new BinaryEntiretyInfoModel();
                sourceFile.FileName = sourceName;
                sourceFile.FileSize = getMergedSize(sourceName);
                _mergedFiles.Add(sourceFile);
            }
        }

        /// <summary>
        /// 把相同源文件名的切片大小加和
        /// </summary>
        /// <param name="sourceName"></param>
        /// <returns></returns>
        private long getMergedSize(string sourceName)
        {
            List<long> sliceSizes =
                _sourceFiles.Where(file => file.MergedFileName == sourceName).
                Select(file => file.FileSize).ToList();
            return sliceSizes.Sum();
        }
    }
}
