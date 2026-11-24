using DTFApp.Models;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Helpers.BlockRenderers
{
    public class HeaderRenderer : BlockRenderer
    {
        public override UIElement Render(Block block)
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
    }
}
