using System;

using UWP_FileSliceAndMerge_Prism.ViewModels;

using Windows.UI.Xaml.Controls;

namespace UWP_FileSliceAndMerge_Prism.Views
{
    public sealed partial class TabbedPivotPage : Page
    {
        private TabbedPivotViewModel ViewModel => DataContext as TabbedPivotViewModel;

        public TabbedPivotPage()
        {
            InitializeComponent();
        }
    }
}
