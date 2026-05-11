using DTFApp.Helpers;
using DTFApp.Models;
using DTFApp.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using RefreshContainer = Microsoft.UI.Xaml.Controls.RefreshContainer;
using RefreshRequestedEventArgs = Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs;

namespace DTFApp.Views
{
    public sealed partial class NewsPage : Page
    {
        public NewsViewModel ViewModel { get; } = new NewsViewModel();

        public NewsPage()
        {
            this.InitializeComponent();
            Loaded += NewsPage_Loaded;
            NewsListView.Loaded += NewsListView_Loaded;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void NewsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.NewsItems.Count == 0)
            {
                await ViewModel.LoadNewsAsync();
            }
        }

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using (args.GetDeferral())
            {
                await ViewModel.RefreshAsync();
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
            var scrollViewer = VisualTreeExtensions.FindScrollViewer(NewsListView);
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
            var scrollViewer = VisualTreeExtensions.FindScrollViewer(NewsListView);
            double oldOffset = scrollViewer?.VerticalOffset ?? 0;
            bool wasNearBottom = scrollViewer != null && scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 200;

            await Task.Delay(50);
            await ViewModel.LoadNewsAsync();

            if (scrollViewer != null && ViewModel.OldCount > 0 && wasNearBottom)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    var sv = VisualTreeExtensions.FindScrollViewer(NewsListView);
                    if (sv != null)
                    {
                        sv.ChangeView(null, oldOffset, null);
                    }
                });
            }
        }
    }
}
