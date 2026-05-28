using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace DTFApp.Models
{
    public class CommentsResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("result")]
        public CommentsResult Result { get; set; }
    }

    public class CommentsResult
    {
        [JsonProperty("items")]
        public Comment[] Items { get; set; }
    }

    public class Comment
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("author")]
        public CommentAuthor Author { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("lastModificationDate")]
        public long LastModificationDate { get; set; }

        [JsonProperty("media")]
        public JArray Media { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("replyTo")]
        public long ReplyTo { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("textMd")]
        public string TextMd { get; set; }
    }

    public class CommentAuthor
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("avatar")]
        public CommentAvatar Avatar { get; set; }

        public string AvatarUrl
        {
            get
            {
                var uuid = Avatar?.Data?.Uuid;
                if (string.IsNullOrEmpty(uuid)) return null;
                if (uuid.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return uuid;
                return $"https://leonardo.osnova.io/{uuid}/-/scale_crop/48x/";
            }
        }
    }

    public class CommentAvatar
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public CommentMediaData Data { get; set; }
    }

    public class CommentMediaAttachment
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Hostname { get; set; }
        public bool IsLink => Type == "link";
    }

    public class CommentMediaData
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
