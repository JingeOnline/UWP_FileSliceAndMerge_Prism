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

            string text = "123_456-((789))";
            string result1 = "-((789))";


            string result = getMergedNameFromSliceName(text);
            Assert.AreEqual(result1, result);
        }

        private string getMergedNameFromSliceName(string name)
        {
            string pattern1 = "[_|\\-|\\(| ]*[\\d]+[\\)]*";
            Match match = Regex.Match(name, pattern1, RegexOptions.RightToLeft);
            return match.Value;
        }
    }
}
