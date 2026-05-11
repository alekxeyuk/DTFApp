using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace DTFApp
{
    public sealed partial class EntryPage : Page
    {
        private readonly HttpClient _httpClient;
        private long _entryId;
        private readonly Grid _fullscreenOverlay = new Grid
        {
            Visibility = Visibility.Collapsed,
            Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black)
        };

        private readonly Image _fullscreenImage = new Image
        {
            Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        private ScrollViewer _fullscreenZoomViewer;

        public EntryPage()
        {
            this.InitializeComponent();
            _httpClient = new HttpClient();
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
                        var renderer = new BlockRendererFactory(ShowFullscreenImage);
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
                Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Black) { Opacity = 0.6 },
                Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.White),
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

    public class BlockRendererFactory
    {
        private readonly Dictionary<string, Func<Block, UIElement>> _renderers;
        private readonly Action<string> _onImageTapped;

        public BlockRendererFactory(Action<string> onImageTapped = null)
        {
            _onImageTapped = onImageTapped;
            _renderers = new Dictionary<string, Func<Block, UIElement>>
            {
                { "text", RenderText },
                { "list", RenderList },
                { "quote", RenderQuote },
                { "media", RenderMedia },
                { "header", RenderHeader },
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

        private struct HtmlTag
        {
            public string Name { get; set; }
            public bool IsClosing { get; set; }
            public bool IsSelfClosing { get; set; }
            public string Href { get; set; }
        }

        private UIElement RenderText(Block block)
        {
            var html = block.Data?.Text;
            if (string.IsNullOrEmpty(html)) return null;

            var richBlock = new RichTextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            };

            int i = 0;
            while (i < html.Length)
            {
                var tag = ExtractTag(html, ref i);
                if (tag.HasValue && tag.Value.Name == "p")
                {
                    var t = tag.Value;
                    var inlines = new List<Inline>();
                    ParseInlines(html, ref i, inlines, "p");
                    var para = new Paragraph();
                    foreach (var inline in inlines)
                        para.Inlines.Add(inline);
                    if (t.IsSelfClosing)
                    {
                        para.Inlines.Add(new LineBreak());
                    }
                    richBlock.Blocks.Add(para);
                }
                else
                {
                    i++;
                }
            }

            if (richBlock.Blocks.Count == 0)
            {
                var fallbackInlines = new List<Inline>();
                ParseInlines(html, ref i, fallbackInlines, null);
                var fallbackPara = new Paragraph();
                foreach (var inline in fallbackInlines)
                    fallbackPara.Inlines.Add(inline);
                richBlock.Blocks.Add(fallbackPara);
            }

            return richBlock;
        }

        private HtmlTag? ExtractTag(string html, ref int i)
        {
            int start = html.IndexOf('<', i);
            if (start < 0) return null;

            int end = html.IndexOf('>', start);
            if (end < 0) return null;

            i = end + 1;
            var content = html.Substring(start + 1, end - start - 1).Trim();

            bool isClosing = content.StartsWith("/");
            if (isClosing) content = content.Substring(1).Trim();

            bool isSelfClosing = content.EndsWith("/");
            if (isSelfClosing) content = content.TrimEnd('/').TrimEnd();

            var parts = content.Split(new[] { ' ' }, 2);
            var name = parts[0].ToLowerInvariant();

            string href = null;
            if (name == "a" && parts.Length > 1)
            {
                var match = Regex.Match(parts[1], @"href=""([^""]+)""");
                if (match.Success) href = match.Groups[1].Value;
            }

            return new HtmlTag { Name = name, IsClosing = isClosing, IsSelfClosing = isSelfClosing, Href = href };
        }

        private void ParseInlines(string html, ref int i, IList<Inline> output, string closeTag)
        {
            while (i < html.Length)
            {
                if (html[i] == '<')
                {
                    var tag = ExtractTag(html, ref i);
                    if (tag == null) { i++; continue; }

                    if (closeTag != null && tag.Value.IsClosing && tag.Value.Name == closeTag)
                        return;

                    switch (tag.Value.Name)
                    {
                        case "br":
                            output.Add(new LineBreak());
                            break;
                        case "b":
                        case "strong":
                            {
                                var bold = new Bold();
                                var sub = new List<Inline>();
                                ParseInlines(html, ref i, sub, tag.Value.Name);
                                foreach (var inline in sub)
                                    bold.Inlines.Add(inline);
                                output.Add(bold);
                                break;
                            }
                        case "i":
                        case "em":
                            {
                                var italic = new Italic();
                                var sub = new List<Inline>();
                                ParseInlines(html, ref i, sub, tag.Value.Name);
                                foreach (var inline in sub)
                                    italic.Inlines.Add(inline);
                                output.Add(italic);
                                break;
                            }
                        case "a":
                            {
                                var link = new Hyperlink();
                                if (tag.Value.Href != null)
                                    link.NavigateUri = new Uri(tag.Value.Href);
                                var sub = new List<Inline>();
                                ParseInlines(html, ref i, sub, tag.Value.Name);
                                foreach (var inline in sub)
                                    link.Inlines.Add(inline);
                                output.Add(link);
                                break;
                            }
                    }
                }
                else
                {
                    int next = html.IndexOf('<', i);
                    if (next < 0) next = html.Length;
                    if (closeTag != null)
                    {
                        var closePos = html.IndexOf("</" + closeTag + ">", i);
                        if (closePos >= 0 && closePos < next) next = closePos;
                    }
                    var text = html.Substring(i, next - i);
                    i = next;
                    if (!string.IsNullOrEmpty(text))
                    {
                        output.Add(new Run { Text = WebUtility.HtmlDecode(text) });
                    }
                }
            }
        }

        private UIElement RenderHeader(Block block)
        {
            var data = block.Data;
            if (data == null || string.IsNullOrEmpty(data.Text)) return null;

            int fontSize;
            switch (data.HeaderStyle)
            {
                case "h1": fontSize = 26; break;
                case "h2": fontSize = 22; break;
                case "h3": fontSize = 20; break;
                default: fontSize = 18; break;
            }

            return new TextBlock
            {
                Text = data.Text,
                FontSize = fontSize,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 15, 0, 5)
            };
        }

        private UIElement RenderMedia(Block block)
        {
            var data = block.Data;
            if (data?.Items == null) return null;

            var panel = new StackPanel { Margin = new Thickness(0, 10, 0, 10) };
            int screenWidth = (int)Window.Current.Bounds.Width;

            var itemsArray = data.Items as JArray;
            if (itemsArray == null) return null;

            foreach (var itemToken in itemsArray)
            {
                var jObject = itemToken as JObject;
                if (jObject == null) continue;

                var mediaItem = jObject.ToObject<MediaItem>();
                if (mediaItem?.Image?.Type != "image") continue;

                var imgData = mediaItem.Image.Data;
                if (imgData == null || string.IsNullOrEmpty(imgData.Uuid)) continue;

                var placeholder = new Border
                {
                    Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.DarkGray),
                    Height = 200,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var grid = new Grid();
                grid.Children.Add(placeholder);

                if (imgData.DataType == "gif")
                {
                    var videoUrl = $"https://leonardo.osnova.io/{imgData.Uuid}/-/format/mp4/";
                    var mediaElement = new MediaElement
                    {
                        Source = new Uri(videoUrl),
                        AutoPlay = !imgData.HasAudio,
                        IsMuted = true,
                        AreTransportControlsEnabled = imgData.HasAudio,
                        Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
                        MaxHeight = 400,
                        Margin = new Thickness(0, 5, 0, 5),
                        IsLooping = !imgData.HasAudio,
                    };
                    grid.Children.Add(mediaElement);
                    mediaElement.MediaOpened += (s, e) =>
                    {
                        placeholder.Visibility = Visibility.Collapsed;
                        if (mediaElement.NaturalVideoWidth > 0)
                        {
                            mediaElement.Height = mediaElement.ActualWidth * mediaElement.NaturalVideoHeight / mediaElement.NaturalVideoWidth;
                        }
                    };
                    mediaElement.MediaFailed += (s, e) => placeholder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var url = $"https://leonardo.osnova.io/{imgData.Uuid}/-/scale_crop/{screenWidth}x/";
                    var bitmap = new BitmapImage(new Uri(url));
                    var uuid = imgData.Uuid;
                    var image = new Image
                    {
                        Source = bitmap,
                        Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
                        MaxHeight = 400,
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    image.Tapped += (s, e) => _onImageTapped?.Invoke(uuid);
                    grid.Children.Add(image);
                    bitmap.ImageOpened += (s, e) => placeholder.Visibility = Visibility.Collapsed;
                    bitmap.ImageFailed += (s, e) => placeholder.Visibility = Visibility.Collapsed;
                }

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
                FontStyle = FontStyle.Italic,
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
            var items = data?.Items as JArray;
            if (items == null) return null;

            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            bool isOrdered = data.ListType == "OL";

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i]?.ToString();
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