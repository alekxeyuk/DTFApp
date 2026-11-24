using DTFApp.Models;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Inline = Windows.UI.Xaml.Documents.Inline;
using LineBreak = Windows.UI.Xaml.Documents.LineBreak;
using Paragraph = Windows.UI.Xaml.Documents.Paragraph;

namespace DTFApp.Helpers.BlockRenderers
{
    public class TextRenderer : BlockRenderer
    {
        public override UIElement Render(Block block)
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
    }
}
