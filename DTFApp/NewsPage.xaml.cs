using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace DTFApp
{
    public sealed partial class NewsPage : Page
    {
        private readonly HttpClient _httpClient;
        private readonly ObservableCollection<NewsData> _newsItems;
        private long _lastId;
        private bool _isLoading;
        private bool _hasMoreItems = true;

        public ObservableCollection<NewsData> NewsItems => _newsItems;

        public NewsPage()
        {
            this.InitializeComponent();
            _httpClient = new HttpClient();
            _newsItems = new ObservableCollection<NewsData>();
            _lastId = 0;
            Loaded += NewsPage_Loaded;
            NewsListView.Loaded += NewsListView_Loaded;
        }

        private async void NewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadNewsAsync();
        }

        private async Task LoadNewsAsync()
        {
            if (_isLoading || !_hasMoreItems)
                return;

            _isLoading = true;

            try
            {
                string url = _lastId == 0
                    ? "https://api.dtf.ru/v2.0/news"
                    : $"https://api.dtf.ru/v2.0/news?lastId={_lastId}";

                var response = await _httpClient.GetStringAsync(url);
                var newsResponse = JsonConvert.DeserializeObject<NewsResponse>(response);

                if (newsResponse?.Result?.News != null)
                {
                    foreach (var item in newsResponse.Result.News)
                    {
                        if (item.Data != null)
                        {
                            _newsItems.Add(item.Data);
                        }
                    }

                    _lastId = newsResponse.Result.LastId;
                    _hasMoreItems = newsResponse.Result.News.Length > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading news: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using (args.GetDeferral())
            {
                _newsItems.Clear();
                _lastId = 0;
                _hasMoreItems = true;
                await LoadNewsAsync();
            }
        }

        private async void NewsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NewsData newsItem)
            {
                if (!string.IsNullOrEmpty(newsItem.Url))
                {
                    var uri = new Uri(newsItem.Url);
                    await Launcher.LaunchUriAsync(uri);
                }
            }
        }

        private void NewsListView_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = FindScrollViewer(NewsListView);
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged += async (s, args) =>
                {
                    if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 100)
                    {
                        await LoadMoreAsync();
                    }
                };
            }
        }

        private async Task LoadMoreAsync()
        {
            await LoadNewsAsync();
        }

        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent is ScrollViewer sv)
                return sv;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}