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
using System.Net.Http;

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
            if (vibrationDevice is null)
            {
                return null;
            }
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

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            //Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            //toastDOM.LoadXml("<toast>"
            //                   + "<visual version='1'>"
            //                   + "<binding template='ToastText04'>"
            //                   + "<text id='1'>Heading text</text>"
            //                   + "<text id='2'>First body text</text>"
            //                   + "<text id='3'>Second body text</text>"
            //                   + "</binding>"
            //                   + "</visual>"
            //                   + "</toast>");
            //ToastNotification toast = new ToastNotification(toastDOM);
            //ToastNotificationManager.CreateToastNotifier().Show(toast);

            //await VibrationDevice.RequestAccessAsync();
            //VibrationDevice testVibrationDevice = await VibrationDevice.GetDefaultAsync();
            //SimpleHapticsControllerFeedback feedback = FindFeedback(testVibrationDevice);
            //testVibrationDevice.SimpleHapticsController.SendHapticFeedbackForDuration(feedback, 1, TimeSpan.FromSeconds(1));

            //string pass = PasswordBox.Password;

            //ContentDialog dialog = new ContentDialog()
            //{
            //    Title = "Hello World",
            //    Content = "How to make it vibrate?\n" + pass,
            //    CloseButtonText = "Ok"
            //};

            //await dialog.ShowAsync();

            var httpClient = new HttpClient();
            var postDict = new Dictionary<string, string>
            {
                { "email", UsernameTextBox.Text },
                { "password", PasswordBox.Password }
            };
            var response = await httpClient.PostAsync("https://api.dtf.ru/v3.0/auth/email/login", new FormUrlEncodedContent(postDict));
            var json = await response.Content.ReadAsStringAsync();
            StatusBox.Text = json;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = UsernameTextBox.Text;
            string pass = PasswordBox.Password;

            ContentDialog dialog = new ContentDialog()
            {
                Title = "Register Dialog",
                Content = $"Username: {login}\nPassword: {pass}",
                CloseButtonText = "Close"
            };

            await dialog.ShowAsync();

            var httpClient = new HttpClient();
            var postDict = new Dictionary<string, string>
            {
                { "email", UsernameTextBox.Text },
                { "password", PasswordBox.Password },
                { "name", "Test" }
            };
            var response = await httpClient.PostAsync("https://api.dtf.ru/v3.0/auth/email/register", new FormUrlEncodedContent(postDict));
            var json = await response.Content.ReadAsStringAsync();

            StatusBox.Text = json;
        }

        private void NewsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewsPage));
        }
    }
}
