using System;

using UWP_FileSliceAndMerge_Prism.ViewModels;

using Windows.UI.Xaml.Controls;

namespace UWP_FileSliceAndMerge_Prism.Views
{
    public sealed partial class BinaryFilePage : Page
    {
        private BinaryFileViewModel ViewModel => DataContext as BinaryFileViewModel;

        public BinaryFilePage()
        {
            InitializeComponent();
        }
    }
}
