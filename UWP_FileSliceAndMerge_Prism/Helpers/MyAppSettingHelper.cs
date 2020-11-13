using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWP_FileSliceAndMerge_Prism.Helpers
{
    public static class MyAppSettingHelper
    {
        //应用程序的setting
        public static ApplicationDataContainer AppSetting = ApplicationData.Current.LocalSettings;
    }
}
