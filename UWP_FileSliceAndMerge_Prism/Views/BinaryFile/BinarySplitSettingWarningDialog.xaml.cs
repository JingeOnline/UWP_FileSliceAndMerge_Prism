using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace UWP_FileSliceAndMerge_Prism.Views
{
    public sealed partial class BinarySplitSettingWarningDialog : ContentDialog
    {
        public long ResultFileNumber { get; set; }
        public string TheMaximumNumberOfSlices { get; set; }
        public string CurrentSituation { get; set; }
        public BinarySplitSettingWarningDialog(long resultFileNumber)
        {
            ResultFileNumber = resultFileNumber;
            RequestedTheme = (Window.Current.Content as FrameworkElement).RequestedTheme;
            TheMaximumNumberOfSlices= ResourceLoader.GetForViewIndependentUse().GetString("String_WarningMaximumNumberOfSlices");
            CurrentSituation= ResourceLoader.GetForViewIndependentUse().GetString("String_WarningMaximumNumberOfSlices_1");
            CurrentSituation += resultFileNumber;
            CurrentSituation+= ResourceLoader.GetForViewIndependentUse().GetString("String_WarningMaximumNumberOfSlices_2");
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            
        }
    }
}
