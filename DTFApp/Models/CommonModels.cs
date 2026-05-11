using Newtonsoft.Json;

namespace DTFApp.Models
{
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
}
