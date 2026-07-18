using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

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
        [JsonConverter(typeof(CommentMediaItemConverter))]
        public List<CommentMediaItem> Media { get; set; }

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

        [JsonProperty("badgeId")]
        public string BadgeId { get; set; }

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

    public class CommentMediaItem
    {
        public string Type { get; set; }
        public string Uuid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Hostname { get; set; }
    }

    public class CommentMediaItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(CommentMediaItem) ||
            objectType == typeof(List<CommentMediaItem>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var array = JArray.Load(reader);
                var list = new List<CommentMediaItem>();
                foreach (var element in array)
                {
                    list.Add(ConvertElement(element));
                }
                return list;
            }

            var obj = JObject.Load(reader);
            return ConvertToken(obj);
        }

        private static CommentMediaItem ConvertElement(JToken token)
        {
            if (token is JObject obj)
                return ConvertToken(obj);
            return null;
        }

        private static CommentMediaItem ConvertToken(JObject obj)
        {
            var type = (string)obj["type"];
            var data = obj["data"] as JObject;

            var item = new CommentMediaItem { Type = type };

            if (type == "image" || type == "movie")
            {
                item.Uuid = data != null ? (string)data["uuid"] : null;
            }
            else if (type == "link" && data != null)
            {
                item.Title = WebUtility.HtmlDecode((string)data["title"]);
                item.Description = WebUtility.HtmlDecode((string)data["description"]);
                item.Hostname = WebUtility.HtmlDecode((string)data["hostname"]);

                var image = data["image"] as JObject;
                var imageData = image?["data"] as JObject;
                if (imageData != null)
                    item.Uuid = (string)imageData["uuid"];
            }

            return item;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
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
