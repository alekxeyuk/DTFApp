using DTFApp.Services;
using System;

namespace DTFApp.Helpers
{
    public class BlockRendererContext
    {
        public BlockRendererContext(IDtfApiService apiService, Action<string> onImageTapped)
        {
            ApiService = apiService;
            OnImageTapped = onImageTapped;
        }

        public IDtfApiService ApiService { get; }

        public Action<string> OnImageTapped { get; }
    }
}
