using DTFApp.Models;
using DTFApp.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Inline = Windows.UI.Xaml.Documents.Inline;
using LineBreak = Windows.UI.Xaml.Documents.LineBreak;
using Paragraph = Windows.UI.Xaml.Documents.Paragraph;

namespace DTFApp.Helpers
{
    public class BlockRendererFactory
    {
        private readonly Dictionary<string, Func<Block, UIElement>> _renderers;
        private readonly Action<string> _onImageTapped;
        private readonly IDtfApiService _apiService;

        public BlockRendererFactory(IDtfApiService apiService, Action<string> onImageTapped = null)
        {
            _apiService = apiService;
            _onImageTapped = onImageTapped;
            _renderers = new Dictionary<string, Func<Block, UIElement>>
            {
                { "text", RenderText },
                { "list", RenderList },
                { "quote", RenderQuote },
                { "media", RenderMedia },
                { "header", RenderHeader },
                { "quiz", RenderQuiz },
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
                var tag = HtmlHelper.ExtractTag(html, ref i);
                if (tag.HasValue && tag.Value.Name == "p")
                {
                    var t = tag.Value;
                    var inlines = new List<Inline>();
                    HtmlHelper.ParseInlines(html, ref i, inlines, "p");
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
                HtmlHelper.ParseInlines(html, ref i, fallbackInlines, null);
                var fallbackPara = new Paragraph();
                foreach (var inline in fallbackInlines)
                    fallbackPara.Inlines.Add(inline);
                richBlock.Blocks.Add(fallbackPara);
            }

            return richBlock;
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

            if (!(data.Items is JArray itemsArray)) return null;

            foreach (var itemToken in itemsArray)
            {
                if (!(itemToken is JObject jObject)) continue;

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

            var quoteText = HtmlHelper.StripHtml(data.Text);
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
                    Text = HtmlHelper.StripHtml(data.Subline1),
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

        private UIElement RenderQuiz(Block block)
        {
            var data = block.Data;
            if (data == null || string.IsNullOrEmpty(data.Hash)) return null;

            if (!(data.Items is JObject options)) return null;

            var card = new Border
            {
                Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Transparent),
                BorderBrush = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.LightGray),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 10, 0, 10)
            };

            var stack = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = data.QuizTitle,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(titleBlock);

            var optionIds = new List<string>();
            var fillBars = new List<Border>();
            var pctTexts = new List<TextBlock>();
            var accent = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.DodgerBlue);

            foreach (var prop in options.Properties())
            {
                var optId = prop.Name;
                var optText = prop.Value?.ToString();
                if (string.IsNullOrEmpty(optText)) continue;

                optionIds.Add(optId);

                var textBlock = new TextBlock
                {
                    Text = optText,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 2)
                };

                var pctText = new TextBlock
                {
                    Text = "",
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 0, 2)
                };
                pctTexts.Add(pctText);

                var barFill = new Border
                {
                    Background = accent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 0,
                    Height = 6
                };
                fillBars.Add(barFill);

                var barTrack = new Border
                {
                    Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.LightGray),
                    Height = 6,
                    CornerRadius = new CornerRadius(3),
                    Child = barFill,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                headerGrid.Children.Add(textBlock);
                headerGrid.Children.Add(pctText);
                Grid.SetColumn(pctText, 1);

                stack.Children.Add(headerGrid);
                stack.Children.Add(barTrack);
            }

            card.Child = stack;

            var hash = data.Hash;
            var dispatcher = Window.Current.Dispatcher;
            double barMaxWidth = Window.Current.Bounds.Width - 64;
            Task.Run(async () =>
            {
                try
                {
                    var result = await _apiService.GetQuizResultsAsync(hash);
                    if (result?.Result?.Items == null) return;

                    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        for (int i = 0; i < optionIds.Count; i++)
                        {
                            if (result.Result.Items.TryGetValue(optionIds[i], out var optResult))
                            {
                                fillBars[i].Width = barMaxWidth * optResult.Percentage / 100.0;
                                pctTexts[i].Text = $"{optResult.Percentage}%";
                                if (optResult.IsWinner)
                                {
                                    fillBars[i].Background = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.ForestGreen);
                                }
                            }
                        }
                    });
                }
                catch { }
            });

            return card;
        }

        private UIElement RenderList(Block block)
        {
            var data = block.Data;
            if (!(data?.Items is JArray items)) return null;

            var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            bool isOrdered = data.ListType == "OL";

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i]?.ToString();
                if (string.IsNullOrEmpty(item)) continue;

                var prefix = isOrdered ? $"{i + 1}. " : "• ";
                var textBlock = new TextBlock
                {
                    Text = prefix + HtmlHelper.StripHtml(item),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                panel.Children.Add(textBlock);
            }

            return panel;
        }
    }
}
