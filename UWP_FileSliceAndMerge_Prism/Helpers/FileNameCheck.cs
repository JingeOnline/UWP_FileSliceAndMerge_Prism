using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWP_FileSliceAndMerge_Prism.Helpers
{
    public static class FileNameCheck
    {
        public static char[] invalidChars = new char[] { '\\' , '/' , '*' , '?' , ':' , '\"' , '<' , '>' , '|' };

        public static bool Check(String fileName)
        {
            if (fileName == null)
            {
                return true;
            }
            char[] fileNameChars = fileName.ToCharArray();
            if (fileNameChars.Intersect(invalidChars).Count() != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
