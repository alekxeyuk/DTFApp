using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return Regex.Replace(html, "<[^>]*>", "");
        }
    }
}