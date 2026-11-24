using DTFApp.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Views
{
    public sealed partial class MainPage : Page
    {
        public LoginViewModel ViewModel { get; } = new LoginViewModel();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.Email = UsernameTextBox.Text;
                ViewModel.Password = PasswordBox.Password;
                await ViewModel.LoginAsync();
                StatusBox.Text = ViewModel.StatusText;
            }
            catch (Exception ex)
            {
                StatusBox.Text = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.Email = UsernameTextBox.Text;
                ViewModel.Password = PasswordBox.Password;
                await ViewModel.RegisterAsync();
                StatusBox.Text = ViewModel.StatusText;
            }
            catch (Exception ex)
            {
                StatusBox.Text = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Register error: {ex}");
            }
        }

        private void NewsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewsPage));
        }
    }
}
