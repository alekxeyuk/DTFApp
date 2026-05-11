using Newtonsoft.Json;

namespace DTFApp.Models
{
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
}
