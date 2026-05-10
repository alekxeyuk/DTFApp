using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.Devices.Haptics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
