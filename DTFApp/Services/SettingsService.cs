using Windows.Storage;

namespace DTFApp.Services
{
    public static class SettingsService
    {
        private const string AutoplayGifsKey = "AutoplayGifs";
        private const string LowDataModeKey = "LowDataMode";
        private const string AuthTokenKey = "AuthToken";

        private static ApplicationDataContainer Settings => ApplicationData.Current.LocalSettings;

        public static bool AutoplayGifs
        {
            get
            {
                object value;
                return Settings.Values.TryGetValue(AutoplayGifsKey, out value) && value is bool b ? b : true;
            }
            set => Settings.Values[AutoplayGifsKey] = value;
        }

        public static bool LowDataMode
        {
            get
            {
                object value;
                return Settings.Values.TryGetValue(LowDataModeKey, out value) && value is bool b ? b : false;
            }
            set => Settings.Values[LowDataModeKey] = value;
        }

        public static string AuthToken
        {
            get
            {
                object value;
                return Settings.Values.TryGetValue(AuthTokenKey, out value) ? value as string : null;
            }
            set => Settings.Values[AuthTokenKey] = value;
        }
    }
}
