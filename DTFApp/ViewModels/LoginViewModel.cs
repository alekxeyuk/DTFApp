using DTFApp.Services;
using System;
using System.Threading.Tasks;

namespace DTFApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private string _email;
        private string _password;
        private string _statusText;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public LoginViewModel() : this(new DtfApiService()) { }

        public LoginViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoginAsync()
        {
            try
            {
                StatusText = await _apiService.LoginAsync(Email, Password);
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
        }

        public async Task RegisterAsync()
        {
            try
            {
                StatusText = await _apiService.RegisterAsync(Email, Password, "Test");
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Register error: {ex}");
            }
        }
    }
}
