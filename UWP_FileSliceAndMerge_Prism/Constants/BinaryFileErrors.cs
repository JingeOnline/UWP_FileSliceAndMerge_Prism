using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP_FileSliceAndMerge_Prism.Constants
{
    internal static class BinaryFileErrors
    {
        public const string InvalidNumber = "Error: Input number must be integer and greater than 0";
        public const string InvalidFileName = "Error: On Windows system, the file name can't contain the following characters." +
            "----" + "\\" + "/" + "*" + "?" + ":" + "\"" + "<" + ">" + "|";
        //public const 
    }
}
