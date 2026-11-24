using DTFApp.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace DTFApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private string _email;
        private string _password;
        private string _name;
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

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public LoginViewModel() : this(AppServices.Api) { }

        public LoginViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoginAsync()
        {
            try
            {
                var raw = await _apiService.LoginAsync(Email, Password);
                SettingsService.AuthToken = ExtractToken(raw);
                StatusText = FormatAuthResponse(raw, "Logged in");
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
                if (string.IsNullOrWhiteSpace(Name))
                {
                    StatusText = "Enter name";
                    return;
                }
                var raw = await _apiService.RegisterAsync(Email, Password, Name);
                StatusText = FormatAuthResponse(raw, "Registered");
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Register error: {ex}");
            }
        }

        private static string ExtractToken(string raw)
        {
            try
            {
                return JObject.Parse(raw)?["result"]?["token"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static string FormatAuthResponse(string raw, string successText)
        {
            if (string.IsNullOrWhiteSpace(raw)) return successText;
            try
            {
                var json = JObject.Parse(raw);
                var message = json["message"]?.ToString();
                if (!string.IsNullOrEmpty(message)) return message;
                if (json["result"] != null || json["error"] == null) return successText;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auth response parse error: {ex.Message}");
            }
            return successText;
        }
    }
}
