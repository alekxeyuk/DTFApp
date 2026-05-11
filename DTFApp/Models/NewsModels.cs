using Newtonsoft.Json;

namespace DTFApp.Models
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
}
