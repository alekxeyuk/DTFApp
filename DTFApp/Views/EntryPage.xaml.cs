using DTFApp.Helpers;
using DTFApp.Services;
using DTFApp.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DTFApp.Views
{
    public sealed partial class EntryPage : Page
    {
        private readonly IDtfApiService _apiService;
        private long _entryId;
        private readonly Grid _fullscreenOverlay = new Grid
        {
            Visibility = Visibility.Collapsed,
            Background = new SolidColorBrush(Windows.UI.Colors.Black)
        };

        private readonly Image _fullscreenImage = new Image
        {
            Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private ScrollViewer _fullscreenZoomViewer;

        public EntryViewModel ViewModel { get; } = new EntryViewModel();

        public EntryPage()
        {
            this.InitializeComponent();
            _apiService = new DtfApiService();
            SetupFullscreenOverlay();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is long id)
            {
                _entryId = id;
                LoadEntryAsync(id);
            }
        }

        protected override void OnNavigatingFrom(Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (_fullscreenOverlay.Visibility == Visibility.Visible)
            {
                HideFullscreenImage();
                e.Cancel = true;
            }
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            if (_fullscreenOverlay.Visibility == Visibility.Visible)
            {
                HideFullscreenImage();
                e.Handled = true;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void LoadEntryAsync(long id)
        {
            try
            {
                await ViewModel.LoadEntryAsync(id);

                if (ViewModel.Entry?.Result != null)
                {
                    var titleBlock = new TextBlock
                    {
                        Text = ViewModel.Title,
                        FontSize = 24,
                        FontWeight = Windows.UI.Text.FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    BlocksPanel.Children.Add(titleBlock);

                    if (ViewModel.Blocks != null)
                    {
                        var renderer = new BlockRendererFactory(_apiService, ShowFullscreenImage);
                        foreach (var block in ViewModel.Blocks)
                        {
                            var element = renderer.Render(block);
                            if (element != null)
                            {
                                BlocksPanel.Children.Add(element);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading entry: {ex.Message}");
            }
        }

        private void SetupFullscreenOverlay()
        {
            var closeButton = new Button
            {
                Content = "✕",
                FontSize = 24,
                Width = 48,
                Height = 48,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 10, 0),
                Background = new SolidColorBrush(Windows.UI.Colors.Black) { Opacity = 0.6 },
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
                BorderThickness = new Thickness(0)
            };
            closeButton.Click += (s, e) => HideFullscreenImage();

            _fullscreenZoomViewer = new ScrollViewer
            {
                Content = _fullscreenImage,
                ZoomMode = ZoomMode.Enabled,
                MinZoomFactor = 1.0f,
                MaxZoomFactor = 4.0f,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var bgGrid = new Grid();
            bgGrid.Children.Add(_fullscreenZoomViewer);
            bgGrid.Children.Add(closeButton);

            _fullscreenOverlay.Children.Add(bgGrid);
            var root = (Grid)this.Content;
            root.Children.Add(_fullscreenOverlay);
            Grid.SetRowSpan(_fullscreenOverlay, 2);
        }

        private void ShowFullscreenImage(string uuid)
        {
            var url = $"https://leonardo.osnova.io/{uuid}/";
            var bounds = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds;
            _fullscreenImage.MaxWidth = bounds.Width;
            _fullscreenImage.MaxHeight = bounds.Height;
            _fullscreenImage.Source = new BitmapImage(new Uri(url));
            _fullscreenOverlay.Visibility = Visibility.Visible;
        }

        private void HideFullscreenImage()
        {
            _fullscreenImage.Source = null;
            _fullscreenZoomViewer.ChangeView(null, null, 1.0f);
            _fullscreenOverlay.Visibility = Visibility.Collapsed;
        }
    }
}
