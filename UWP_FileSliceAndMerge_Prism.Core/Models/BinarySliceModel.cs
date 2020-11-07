using System;
using System.Collections.Generic;
using System.Text;

namespace UWP_FileSliceAndMerge_Prism.Core.Models
{
    public class BinarySliceModel
    {
        public string SourceFileName { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public bool IsDone { get; set; }
    }
}
