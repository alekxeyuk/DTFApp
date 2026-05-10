using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace DTFApp
{
    public sealed partial class EntryPage : Page
    {
        private readonly HttpClient _httpClient;
        private long _entryId;

        public EntryPage()
        {
            this.InitializeComponent();
            _httpClient = new HttpClient();
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

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private async void LoadEntryAsync(long id)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://api.dtf.ru/v2.0/content?id={id}");
                var entryResponse = JsonConvert.DeserializeObject<EntryResponse>(response);

                if (entryResponse?.Result != null)
                {
                    var titleBlock = new TextBlock
                    {
                        Text = entryResponse.Result.Title,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    BlocksPanel.Children.Add(titleBlock);

                    if (entryResponse.Result.Blocks != null)
                    {
                        var renderer = new BlockRendererFactory();
                        foreach (var block in entryResponse.Result.Blocks)
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
    }

    public class BlockRendererFactory
    {
        private readonly Dictionary<string, Func<Block, UIElement>> _renderers;

        public BlockRendererFactory()
        {
            _renderers = new Dictionary<string, Func<Block, UIElement>>
            {
                { "text", RenderText },
                { "list", RenderList },
                { "quote", RenderQuote },
                { "media", RenderMedia },
            };
        }

        public UIElement Render(Block block)
        {
            if (_renderers.TryGetValue(block.Type, out var render))
            {
                return render(block);
            }
            return null;
        }

        private UIElement RenderText(Block block)
        {
            var textBlock = new TextBlock
            {
                Text = StripHtml(block.Data?.Text),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            };
            return textBlock;
        }

        private UIElement RenderMedia(Block block)
        {
            var data = block.Data;
            if (data?.Items == null) return null;

            var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 10) };
            int screenWidth = (int)Window.Current.Bounds.Width;

            foreach (var itemObj in data.Items)
            {
                var jObject = itemObj as JObject;
                if (jObject == null) continue;

                var mediaItem = jObject.ToObject<MediaItem>();
                if (mediaItem?.Image?.Type != "image") continue;

                var imgData = mediaItem.Image.Data;
                if (imgData == null || string.IsNullOrEmpty(imgData.Uuid)) continue;

                var url = $"https://leonardo.osnova.io/{imgData.Uuid}/-/scale_crop/{screenWidth}x/";
                var bitmap = new BitmapImage(new Uri(url));

                var placeholder = new Border
                {
                    Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.DarkGray),
                    Height = 200,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var image = new Image
                {
                    Source = bitmap,
                    Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
                    MaxHeight = 400,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var grid = new Grid();
                grid.Children.Add(placeholder);
                grid.Children.Add(image);

                bitmap.ImageOpened += (s, e) => placeholder.Visibility = Visibility.Collapsed;
                bitmap.ImageFailed += (s, e) => placeholder.Visibility = Visibility.Collapsed;

                panel.Children.Add(grid);
            }

            return panel.Children.Count > 0 ? panel : null;
        }

        private UIElement RenderQuote(Block block)
        {
            var data = block.Data;
            if (data == null) return null;

            var quoteText = StripHtml(data.Text);
            if (string.IsNullOrEmpty(quoteText)) return null;

            var stack = new StackPanel();

            var textBlock = new TextBlock
            {
                Text = quoteText,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(12, 5, 0, 5)
            };
            stack.Children.Add(textBlock);

            if (!string.IsNullOrEmpty(data.Subline1))
            {
                var sublineBlock = new TextBlock
                {
                    Text = StripHtml(data.Subline1),
                    FontSize = 13,
                    Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Gray),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(12, 0, 0, 5)
                };
                stack.Children.Add(sublineBlock);
            }

            var border = new Border
            {
                Child = stack,
                BorderBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.LightGray),
                BorderThickness = new Thickness(3, 0, 0, 0),
                Margin = new Thickness(0, 10, 0, 10),
                Padding = new Thickness(8, 0, 0, 0)
            };

            return border;
        }

        private UIElement RenderList(Block block)
        {
            var data = block.Data;
            if (data?.Items == null) return null;

            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            bool isOrdered = data.ListType == "OL";

            for (int i = 0; i < data.Items.Length; i++)
            {
                var item = data.Items[i]?.ToString();
                if (string.IsNullOrEmpty(item)) continue;

                var prefix = isOrdered ? $"{i + 1}. " : "• ";
                var textBlock = new TextBlock
                {
                    Text = prefix + StripHtml(item),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                panel.Children.Add(textBlock);
            }

            return panel;
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return Regex.Replace(html, "<[^>]*>", "");
        }
    }
}