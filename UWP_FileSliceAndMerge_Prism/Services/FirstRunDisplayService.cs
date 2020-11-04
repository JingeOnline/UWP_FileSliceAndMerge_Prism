using System;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.Helpers;

using UWP_FileSliceAndMerge_Prism.Views;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UWP_FileSliceAndMerge_Prism.Services
{
    public class FirstRunDisplayService : IFirstRunDisplayService
    {
        private static bool shown = false;

        public async Task ShowIfAppropriateAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    if (SystemInformation.IsFirstRun && !shown)
                    {
                        shown = true;
                        var dialog = new FirstRunDialog();
                        await dialog.ShowAsync();
                    }
                });
        }
    }
}
