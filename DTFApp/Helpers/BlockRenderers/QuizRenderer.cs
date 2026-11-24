using DTFApp.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Helpers.BlockRenderers
{
    public class QuizRenderer : BlockRenderer
    {
        private readonly BlockRendererContext _context;

        public QuizRenderer(BlockRendererContext context)
        {
            _context = context;
        }

        public override UIElement Render(Block block)
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
                    var result = await _context.ApiService.GetQuizResultsAsync(hash);
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
    }
}
