using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Haptics;
using Windows.UI.Notifications;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace DTFApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private SimpleHapticsControllerFeedback FindFeedback(VibrationDevice vibrationDevice)
        {
            foreach (var feedback in vibrationDevice.SimpleHapticsController.SupportedFeedback)
            {
                // BuzzContinuous feedback is equivalent to vibration feedback
                if (feedback.Waveform == KnownSimpleHapticsControllerWaveforms.BuzzContinuous)
                {
                    return feedback;
                }
            }

            // No supported feedback type has been found
            return null;
        }

        private async void Button_click(object sender, RoutedEventArgs e)
        {
            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            toastDOM.LoadXml("<toast>"
                               + "<visual version='1'>"
                               + "<binding template='ToastText04'>"
                               + "<text id='1'>Heading text</text>"
                               + "<text id='2'>First body text</text>"
                               + "<text id='3'>Second body text</text>"
                               + "</binding>"
                               + "</visual>"
                               + "</toast>");
            ToastNotification toast = new ToastNotification(toastDOM);
            ToastNotificationManager.CreateToastNotifier().Show(toast);

            await VibrationDevice.RequestAccessAsync();
            VibrationDevice testVibrationDevice = await VibrationDevice.GetDefaultAsync();
            SimpleHapticsControllerFeedback feedback = FindFeedback(testVibrationDevice);
            testVibrationDevice.SimpleHapticsController.SendHapticFeedbackForDuration(feedback, 1, TimeSpan.FromSeconds(1));

            ContentDialog dialog = new ContentDialog()
            {
                Title = "Hello World",
                Content = "How to make it vibrate?",
                CloseButtonText = "Ok"
            };
            
            await dialog.ShowAsync();
        }
    }
}
