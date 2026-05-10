using Newtonsoft.Json;

namespace DTFApp
{
    public class NewsResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public NewsResult Result { get; set; }
    }

    public class NewsResult
    {
        [JsonProperty("news")]
        public NewsItem[] News { get; set; }

        [JsonProperty("lastId")]
        public long LastId { get; set; }
    }

    public class NewsItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public NewsData Data { get; set; }
    }

    public class NewsData
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("subsiteId")]
        public long SubsiteId { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("dateModified")]
        public long DateModified { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("counters")]
        public Counters Counters { get; set; }

        [JsonProperty("ogDescription")]
        public string OgDescription { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("subsite")]
        public Subsite Subsite { get; set; }

        [JsonProperty("likes")]
        public Likes Likes { get; set; }
    }

    public class Counters
    {
        [JsonProperty("comments")]
        public long Comments { get; set; }

        [JsonProperty("favorites")]
        public long Favorites { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }

        [JsonProperty("hits")]
        public long Hits { get; set; }

        [JsonProperty("reactions")]
        public long Reactions { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public class Subsite
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }
    }

    public class Likes
    {
        [JsonProperty("counterLikes")]
        public long CounterLikes { get; set; }
    }

    public class EntryResponse
    {
        [JsonProperty("result")]
        public EntryResult Result { get; set; }
    }

    public class EntryResult
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("blocks")]
        public Block[] Blocks { get; set; }
    }

    public class Block
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("cover")]
        public bool Cover { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }

        [JsonProperty("data")]
        public BlockData Data { get; set; }
    }

    public class BlockData
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("items")]
        public object[] Items { get; set; }

        [JsonProperty("type")]
        public string ListType { get; set; }

        [JsonProperty("subline1")]
        public string Subline1 { get; set; }
    }

    public class MediaItem
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image")]
        public MediaImage Image { get; set; }
    }

    public class MediaImage
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public MediaImageData Data { get; set; }
    }

    public class MediaImageData
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("type")]
        public string DataType { get; set; }

        [JsonProperty("has_audio")]
        public bool HasAudio { get; set; }
    }

    public abstract class BlockRenderer
    {
        public abstract Windows.UI.Xaml.UIElement Render(Block block);
    }

    public class TextBlockRenderer : BlockRenderer
    {
        public override Windows.UI.Xaml.UIElement Render(Block block)
        {
            var textBlock = new Windows.UI.Xaml.Controls.TextBlock
            {
                Text = StripHtml(block.Data.Text),
                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                Margin = new Windows.UI.Xaml.Thickness(0, 5, 0, 5)
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