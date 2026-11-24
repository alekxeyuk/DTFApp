using DTFApp.Models;
using DTFApp.Services;
using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace DTFApp.Helpers.BlockRenderers
{
    public class MediaRenderer : BlockRenderer
    {
        private readonly BlockRendererContext _context;

        public MediaRenderer(BlockRendererContext context)
        {
            _context = context;
        }

        public override UIElement Render(Block block)
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
                    var videoUrl = $"{ApiConstants.LeonardoBaseUrl}/{imgData.Uuid}/-/format/mp4/";
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
                    mediaElement.MediaFailed += (s, e) => ShowLoadError(placeholder);

                    grid.EffectiveViewportChanged += (s, e) =>
                    {
                        bool inView = e.EffectiveViewport.Height > 0
                            && e.EffectiveViewport.Top < Window.Current.Bounds.Height
                            && e.EffectiveViewport.Bottom > 0;
                        if (inView && !imgData.HasAudio)
                        {
                            mediaElement.Play();
                        }
                        else
                        {
                            mediaElement.Pause();
                        }
                    };
                }
                else
                {
                    var url = ApiConstants.LeonardoScaleCropUrl(imgData.Uuid, screenWidth + "x");
                    var bitmap = new BitmapImage(new Uri(url));
                    var uuid = imgData.Uuid;
                    var image = new Image
                    {
                        Source = bitmap,
                        Stretch = Windows.UI.Xaml.Media.Stretch.Uniform,
                        MaxHeight = 400,
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    image.Tapped += (s, e) => _context.OnImageTapped?.Invoke(uuid);
                    grid.Children.Add(image);
                    bitmap.ImageOpened += (s, e) => placeholder.Visibility = Visibility.Collapsed;
                    bitmap.ImageFailed += (s, e) => ShowLoadError(placeholder);
                }

                panel.Children.Add(grid);
            }

            return panel.Children.Count > 0 ? panel : null;
        }

        private static void ShowLoadError(Border placeholder)
        {
            placeholder.Child = new TextBlock
            {
                Text = "Failed to load media",
                Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center
            };
        }
    }
}
