using DTFApp.Models;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Helpers.BlockRenderers
{
    public class ListRenderer : BlockRenderer
    {
        public override UIElement Render(Block block)
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
