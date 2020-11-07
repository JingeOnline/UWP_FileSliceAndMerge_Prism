using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UWP_FileSliceAndMerge_Prism.Core.Models;
using UWP_FileSliceAndMerge_Prism.Core.Services;

namespace UWP_FileSliceAndMerge_Prism.Tests.MSTest
{
    [TestClass]
    public class PreviewOutputServiceTest
    {
        [TestMethod]
        public void TestGetPreviewSlicesByNumber()
        {
            //string name = getSliceName("jinge.jpg", "{*}_{#}", 1);
            //Assert.AreEqual(name, "jinge_1.jpg");
            int sliceNumber=3;
            string namingRule= "{*}_{#}";
            int indexStartWith=1;
            List<BinarySliceModel> sourceFils = new List<BinarySliceModel>()
            {
                new BinarySliceModel(){FileName="test1.jpg",FileSize=300, },
                new BinarySliceModel(){FileName="test2.jpg",FileSize=400,},
                new BinarySliceModel(){FileName="test3.jpg",FileSize=9001,}
            };

            PreviewOutputService previewOutputService = 
                new PreviewOutputService(sliceNumber, namingRule, indexStartWith);
            List<BinarySliceModel> list = previewOutputService.GetPreviewSlicesByNumber(sourceFils);

            Assert.AreEqual(list.Count,9);
            Assert.AreEqual(list.Last().FileSize, 3001);
        }

    }
}
