using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using Windows.UI.Notifications;

namespace UWP_FileSliceAndMerge_Prism.Services
{
    internal partial class ToastNotificationsService : IToastNotificationsService
    {
        public void ShowToastNotification(ToastNotification toastNotification)
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
            }
            catch (Exception)
            {
                // TODO WTS: Adding ToastNotification can fail in rare conditions, please handle exceptions as appropriate to your scenario.
            }
        }

        public void ShowTaskFinishToast(string toastTitle,string toastContent)
        {
            // Create the toast content
            ToastContent content = new ToastContent();
            // 当用户点击该Toast的时候，会把该参数传给app，让app能够响应对应的操作。
            content.Launch = "TaskFinish";

            //Toast显示内容
            content.Visual = new ToastVisual();
            content.Visual.BindingGeneric = new ToastBindingGeneric()
            {
                Children =
                {
                      new AdaptiveText(){Text=toastTitle},
                      new AdaptiveText(){Text=toastContent}
                }
            };

            //Toast按钮
            ToastActionsCustom toastActions = new ToastActionsCustom();
            toastActions.Buttons.Add(
                new ToastButton("OK","TaskFinishOk") { ActivationType = ToastActivationType.Foreground }
                );
            toastActions.Buttons.Add(
                new ToastButtonDismiss("Cancel")
                );
            content.Actions = toastActions;

            //打包成ToastNotification
            ToastNotification toast = new ToastNotification(content.GetXml());

            //调用上面的方法显示Toast
            ShowToastNotification(toast);
        }
    }
}
