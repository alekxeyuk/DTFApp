using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
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
        private long _lastId;
        private bool _isLoading;
        private bool _hasMoreItems = true;

        public ObservableCollection<NewsData> NewsItems { get; }

        public NewsPage()
        {
            this.InitializeComponent();
            _httpClient = new HttpClient();
            NewsItems = new ObservableCollection<NewsData>();
            _lastId = 0;
            Loaded += NewsPage_Loaded;
            NewsListView.Loaded += NewsListView_Loaded;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void NewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (NewsItems.Count == 0)
            {
                await LoadNewsAsync();
            }
        }

        private async Task LoadNewsAsync()
        {
            if (_isLoading || !_hasMoreItems)
                return;

            _isLoading = true;

            var scrollViewer = FindScrollViewer(NewsListView);
            double oldOffset = scrollViewer?.VerticalOffset ?? 0;
            bool wasNearBottom = scrollViewer != null && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 200;

            try
            {
                string url = _lastId == 0
                    ? "https://api.dtf.ru/v2.0/news"
                    : $"https://api.dtf.ru/v2.0/news?lastId={_lastId}";

                var response = await _httpClient.GetStringAsync(url);
                var newsResponse = JsonConvert.DeserializeObject<NewsResponse>(response);

                if (newsResponse?.Result?.News != null)
                {
                    int oldCount = NewsItems.Count;

                    foreach (var item in newsResponse.Result.News)
                    {
                        if (item.Data != null)
                        {
                            NewsItems.Add(item.Data);
                        }
                    }

                    _lastId = newsResponse.Result.LastId;
                    _hasMoreItems = newsResponse.Result.News.Length > 0;

                    if (scrollViewer != null && oldCount > 0 && wasNearBottom)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                        {
                            var sv = FindScrollViewer(NewsListView);
                            if (sv != null)
                            {
                                sv.ChangeView(null, oldOffset, null);
                            }
                        });
                    }
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
                NewsItems.Clear();
                _lastId = 0;
                _hasMoreItems = true;
                await LoadNewsAsync();
            }
        }

        private void NewsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NewsData newsItem)
            {
                this.Frame.Navigate(typeof(EntryPage), newsItem.Id);
            }
        }

        private void NewsListView_Loaded(object sender, RoutedEventArgs e)
        {
            var scrollViewer = FindScrollViewer(NewsListView);
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged += async (s, args) =>
                {
                    if (!args.IsIntermediate && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 100)
                    {
                        await LoadMoreAsync();
                    }
                };
            }
        }

        private async Task LoadMoreAsync()
        {
            await Task.Delay(50);
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