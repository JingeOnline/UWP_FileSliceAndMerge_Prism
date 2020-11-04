using System;

using UWP_FileSliceAndMerge_Prism.ViewModels;

using Windows.UI.Xaml.Controls;

namespace UWP_FileSliceAndMerge_Prism.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
