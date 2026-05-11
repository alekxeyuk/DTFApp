using Windows.UI.Xaml;

namespace DTFApp.Models
{
    public abstract class BlockRenderer
    {
        public abstract UIElement Render(Block block);
    }

    public class TextBlockRenderer : BlockRenderer
    {
        public override UIElement Render(Block block)
        {
            var textBlock = new Windows.UI.Xaml.Controls.TextBlock
            {
                Text = StripHtml(block.Data.Text),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            };
            return textBlock;
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
        }
    }
}
