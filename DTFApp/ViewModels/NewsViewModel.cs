using DTFApp.Models;
using DTFApp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DTFApp.ViewModels
{
    public class NewsViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private long _lastId;
        private bool _isLoading;
        private bool _hasMoreItems = true;

        public ObservableCollection<NewsData> NewsItems { get; } = new ObservableCollection<NewsData>();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool HasMoreItems
        {
            get => _hasMoreItems;
            set => SetProperty(ref _hasMoreItems, value);
        }

        public int OldCount { get; private set; }

        public NewsViewModel() : this(new DtfApiService()) { }

        public NewsViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadNewsAsync()
        {
            if (_isLoading || !_hasMoreItems) return;
            IsLoading = true;

            try
            {
                var result = await _apiService.GetNewsAsync(_lastId);
                if (result?.News != null)
                {
                    OldCount = NewsItems.Count;
                    foreach (var item in result.News)
                    {
                        if (item.Data != null)
                            NewsItems.Add(item.Data);
                    }
                    _lastId = result.LastId;
                    HasMoreItems = result.News.Length > 0;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshAsync()
        {
            NewsItems.Clear();
            _lastId = 0;
            HasMoreItems = true;
            OldCount = 0;
            await LoadNewsAsync();
        }
    }
}
