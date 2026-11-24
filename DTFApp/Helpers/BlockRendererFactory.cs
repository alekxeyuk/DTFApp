using DTFApp.Helpers.BlockRenderers;
using DTFApp.Models;
using DTFApp.Services;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace DTFApp.Helpers
{
    public class BlockRendererFactory
    {
        private readonly Dictionary<string, BlockRenderer> _renderers;

        public BlockRendererFactory(IDtfApiService apiService, Action<string> onImageTapped = null)
        {
            var context = new BlockRendererContext(apiService, onImageTapped);
            _renderers = new Dictionary<string, BlockRenderer>
            {
                { "text", new TextRenderer() },
                { "list", new ListRenderer() },
                { "quote", new QuoteRenderer() },
                { "media", new MediaRenderer(context) },
                { "header", new HeaderRenderer() },
                { "quiz", new QuizRenderer(context) },
            };
        }

        public UIElement Render(Block block)
        {
            if (block != null && _renderers.TryGetValue(block.Type, out var renderer))
            {
                return renderer.Render(block);
            }
            return null;
        }
    }
}
