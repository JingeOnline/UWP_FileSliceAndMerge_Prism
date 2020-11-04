using Windows.UI.Notifications;

namespace UWP_FileSliceAndMerge_Prism.Services
{
    internal interface IToastNotificationsService
    {
        void ShowToastNotification(ToastNotification toastNotification);

        void ShowToastNotificationSample();
    }
}
