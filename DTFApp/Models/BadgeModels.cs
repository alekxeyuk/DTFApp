using Newtonsoft.Json;

namespace DTFApp.Models
{
    public class BadgeAssetsResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public BadgeAssetsResult Result { get; set; }
    }

    public class BadgeAssetsResult
    {
        [JsonProperty("badges")]
        public BadgeAsset[] Badges { get; set; }
    }

    public class BadgeAsset
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("staticUuid")]
        public string StaticUuid { get; set; }

        [JsonProperty("animatedUuid")]
        public string AnimatedUuid { get; set; }
    }
}
