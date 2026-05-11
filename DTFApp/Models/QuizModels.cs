using Newtonsoft.Json;
using System.Collections.Generic;

namespace DTFApp.Models
{
    public class QuizResultResponse
    {
        [JsonProperty("result")]
        public QuizResult Result { get; set; }
    }

    public class QuizResult
    {
        [JsonProperty("items")]
        public Dictionary<string, QuizOptionResult> Items { get; set; }

        [JsonProperty("winner")]
        public string Winner { get; set; }
    }

    public class QuizOptionResult
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("percentage")]
        public long Percentage { get; set; }

        [JsonProperty("isWinner")]
        public bool IsWinner { get; set; }
    }
}
