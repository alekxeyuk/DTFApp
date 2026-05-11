using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DTFApp.Models
{
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
        public JToken Items { get; set; }

        [JsonProperty("type")]
        public string ListType { get; set; }

        [JsonProperty("subline1")]
        public string Subline1 { get; set; }

        [JsonProperty("style")]
        public string HeaderStyle { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("title")]
        public string QuizTitle { get; set; }
    }
}
