namespace DTFApp.Services
{
    public static class ApiConstants
    {
        public const string ApiBaseUrl = "https://api.dtf.ru";
        public const string LeonardoBaseUrl = "https://leonardo.osnova.io";

        public static string LeonardoScaleCropUrl(string uuid, string size)
        {
            return $"{LeonardoBaseUrl}/{uuid}/-/scale_crop/{size}/";
        }
    }
}
