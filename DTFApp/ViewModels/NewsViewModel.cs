using DTFApp.Models;
using DTFApp.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DTFApp.ViewModels
{
    public class NewsViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private long _lastId;
        private bool _isLoading;
        private bool _hasMoreItems = true;
        private string _errorMessage;
        private bool _hasLoadedNews;

        public BulkObservableCollection<NewsData> NewsItems { get; } = new BulkObservableCollection<NewsData>();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(EmptyVisibility));
                }
            }
        }

        public bool HasMoreItems
        {
            get => _hasMoreItems;
            set => SetProperty(ref _hasMoreItems, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(ErrorVisibility));
                    OnPropertyChanged(nameof(EmptyVisibility));
                }
            }
        }

        public Visibility ErrorVisibility => string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility EmptyVisibility => _hasLoadedNews && !IsLoading && string.IsNullOrWhiteSpace(ErrorMessage) && NewsItems.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        public int OldCount { get; private set; }

        public NewsViewModel() : this(AppServices.Api) { }

        public NewsViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadNewsAsync(CancellationToken ct = default)
        {
            if (_isLoading || !_hasMoreItems) return;
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                var result = await _apiService.GetNewsAsync(_lastId, ct);
                if (result?.News != null)
                {
                    OldCount = NewsItems.Count;
                    NewsItems.AddRange(result.News.Where(item => item.Data != null).Select(item => item.Data));
                    _lastId = result.LastId;
                    HasMoreItems = result.News.Length > 0;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error loading news: {ex}");
            }
            finally
            {
                _hasLoadedNews = true;
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
