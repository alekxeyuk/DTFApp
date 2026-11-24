using DTFApp.Models;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Helpers.BlockRenderers
{
    public class QuoteRenderer : BlockRenderer
    {
        public override UIElement Render(Block block)
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
    }
}
