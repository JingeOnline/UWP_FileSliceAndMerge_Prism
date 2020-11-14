using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UWP_FileSliceAndMerge_Prism.Models;
using UWP_FileSliceAndMerge_Prism.Services;

namespace UWP_FileSliceAndMerge_Prism.Tests.MSTest
{
    [TestClass]
    public class PreviewOutputServiceTest
    {

        [TestMethod]
        public void test()
        {
            //string text = "123_456  (789)";
            //string result1 = "  (789)";

            //string text = "123_456_(789)";
            //string result1 = "_(789)";

            //string text = "123_456-((789))";
            //string result1 = "-((789))";

            //string text = "四海图库_2 (3)";
            //string match = " (3)";

            string text = "四海图库_2_(3)";
            string match = "四海图库_2";

            string result = getMergedNameFromSliceName(text);
            Assert.AreEqual(match, result);
        }

        private string getMergedNameFromSliceName(string name)
        {
            string fileName = null;
            string pattern = "[_|\\-|\\(| ]*[\\d]+[\\)]*";
            Match match = Regex.Match(name, pattern, RegexOptions.RightToLeft);
            if (match != null)
            {
                int i = name.LastIndexOf(match.Value);
                fileName = name.Substring(0, i);
            }
            return fileName;
        }
    }
}
