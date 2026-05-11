using DTFApp.ViewModels;
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
            ViewModel.Email = UsernameTextBox.Text;
            ViewModel.Password = PasswordBox.Password;
            await ViewModel.LoginAsync();
            StatusBox.Text = ViewModel.StatusText;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Email = UsernameTextBox.Text;
            ViewModel.Password = PasswordBox.Password;
            await ViewModel.RegisterAsync();
            StatusBox.Text = ViewModel.StatusText;
        }

        private void NewsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewsPage));
        }
    }
}
