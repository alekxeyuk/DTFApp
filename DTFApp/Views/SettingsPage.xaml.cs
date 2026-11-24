using DTFApp.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Views
{
    public sealed partial class SettingsPage : Page
    {
        private bool _initialized;

        public SettingsPage()
        {
            InitializeComponent();
            AutoplayGifsToggle.IsOn = SettingsService.AutoplayGifs;
            LowDataToggle.IsOn = SettingsService.LowDataMode;
            _initialized = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void AutoplayGifsToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_initialized)
            {
                SettingsService.AutoplayGifs = AutoplayGifsToggle.IsOn;
            }
        }

        private void LowDataToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_initialized)
            {
                SettingsService.LowDataMode = LowDataToggle.IsOn;
            }
        }
    }
}
