using DTFApp.Helpers;
using DTFApp.Models;
using DTFApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Windows.UI.Xaml;

namespace DTFApp.ViewModels
{
    public class CommentViewItem : BaseViewModel
    {
        private const int MaxIndentLevel = 5;
        private bool _isExpanded = true;
        private bool _hasReplies;
        private bool _isHiddenByCollapse;

        public CommentViewItem(Comment comment)
        {
            Comment = comment;
            MediaAttachments = CreateMediaAttachments(comment?.Media);
            Text = WebUtility.HtmlDecode(HtmlHelper.StripHtml(comment?.Text ?? ""));
            TextSegments = ParseTextSegments(Text);

            var level = comment?.Level ?? 0;
            if (level < 0) level = 0;
            if (level > MaxIndentLevel) level = MaxIndentLevel;
            IndentMargin = new Thickness(level * 10, level == 0 ? 12 : 0, 0, 0);
        }

        public Comment Comment { get; }
        public ObservableCollection<CommentMediaAttachmentViewItem> MediaAttachments { get; }
        public List<CommentViewItem> Children { get; } = new List<CommentViewItem>();

        public long Id => Comment?.Id ?? 0;

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool HasReplies
        {
            get => _hasReplies;
            set
            {
                if (SetProperty(ref _hasReplies, value))
                {
                    OnPropertyChanged(nameof(ExpandToggleVisibility));
                }
            }
        }

        public bool IsHiddenByCollapse
        {
            get => _isHiddenByCollapse;
            set
            {
                if (SetProperty(ref _isHiddenByCollapse, value))
                {
                    OnPropertyChanged(nameof(RowVisibility));
                }
            }
        }

        public Visibility RowVisibility => _isHiddenByCollapse ? Visibility.Collapsed : Visibility.Visible;

        public Visibility ExpandToggleVisibility => _hasReplies ? Visibility.Visible : Visibility.Collapsed;

        public string AuthorName => Comment?.Author?.Name ?? "Unknown";
        public string AvatarUrl => Comment?.Author?.AvatarUrl;
        public string BadgeUrl => BadgeCacheService.GetBadgeUrl(Comment?.Author?.BadgeId);
        public Visibility BadgeVisibility => string.IsNullOrWhiteSpace(BadgeUrl) ? Visibility.Collapsed : Visibility.Visible;
        public string TimeAgo => FormatTimeAgo(Comment?.Date ?? 0);
        public string NicknameText
        {
            get
            {
                var nickname = Comment?.Author?.Nickname;
                return string.IsNullOrWhiteSpace(nickname) ? "" : $"@{nickname}";
            }
        }
        public Visibility NicknameVisibility => string.IsNullOrWhiteSpace(NicknameText) ? Visibility.Collapsed : Visibility.Visible;
        public string Text { get; }

        public Visibility TextVisibility => string.IsNullOrWhiteSpace(Text) ? Visibility.Collapsed : Visibility.Visible;

        public List<CommentTextSegment> TextSegments { get; }

        public Visibility TextSegmentsVisibility => TextSegments == null || TextSegments.Count == 0
            ? Visibility.Collapsed
            : Visibility.Visible;

        public Thickness IndentMargin { get; }

        private static string FormatTimeAgo(long unixTime)
        {
            if (unixTime <= 0) return "";

            var commentTime = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            var elapsed = DateTimeOffset.Now - commentTime;
            if (elapsed.TotalSeconds < 0) elapsed = TimeSpan.Zero;

            if (elapsed.TotalMinutes < 1) return "now";
            if (elapsed.TotalHours < 1) return $"{(int)elapsed.TotalMinutes}м";
            if (elapsed.TotalDays < 1) return $"{(int)elapsed.TotalHours}ч";
            if (elapsed.TotalDays < 7) return $"{(int)elapsed.TotalDays}д";
            if (elapsed.TotalDays < 30) return $"{(int)(elapsed.TotalDays / 7)}н";
            if (elapsed.TotalDays < 365) return $"{(int)(elapsed.TotalDays / 30)}мес";
            return $"{(int)(elapsed.TotalDays / 365)}г";
        }

        private static string CompactLineBreaks(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var sb = new System.Text.StringBuilder(text.Length);
            for (int i = 0; i < text.Length;)
            {
                if (i + 1 < text.Length && text[i] == '\r' && text[i + 1] == '\n')
                {
                    int count = 0;
                    while (i + 1 < text.Length && text[i] == '\r' && text[i + 1] == '\n')
                    {
                        count++;
                        i += 2;
                    }

                    sb.Append('\r').Append('\n');
                    if (count % 2 == 0)
                        sb.Append('\r').Append('\n');
                }
                else
                {
                    sb.Append(text[i]);
                    i++;
                }
            }

            return sb.ToString();
        }

        private static List<CommentTextSegment> ParseTextSegments(string text)
        {
            var segments = new List<CommentTextSegment>();
            if (string.IsNullOrWhiteSpace(text)) return segments;

            text = CompactLineBreaks(text);

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool currentIsQuote = false;
            var buffer = new List<string>();

            foreach (var line in lines)
            {
                bool isQuoteLine = line.StartsWith(">");
                string content = isQuoteLine ? line.Substring(1).TrimStart() : line;

                if (isQuoteLine != currentIsQuote && buffer.Count > 0)
                {
                    segments.Add(new CommentTextSegment
                    {
                        Text = string.Join("\n", buffer),
                        IsQuote = currentIsQuote
                    });
                    buffer.Clear();
                }

                buffer.Add(content);
                currentIsQuote = isQuoteLine;
            }

            if (buffer.Count > 0)
            {
                segments.Add(new CommentTextSegment
                {
                    Text = string.Join("\n", buffer),
                    IsQuote = currentIsQuote
                });
            }

            return segments;
        }

        private static ObservableCollection<CommentMediaAttachmentViewItem> CreateMediaAttachments(List<CommentMediaItem> media)
        {
            var attachments = new ObservableCollection<CommentMediaAttachmentViewItem>();
            if (media == null) return attachments;

            foreach (var item in media)
            {
                if (item.Type == "image" || item.Type == "movie")
                {
                    var uuid = item.Uuid;
                    if (string.IsNullOrEmpty(uuid)) continue;

                    var url = uuid.StartsWith("http")
                        ? uuid
                        : $"https://leonardo.osnova.io/{uuid}/-/scale_crop/480x/";

                    attachments.Add(new CommentMediaAttachmentViewItem
                    {
                        Type = item.Type,
                        Url = url
                    });
                }
                else if (item.Type == "link")
                {
                    var uuid = item.Uuid;
                    var imageUrl = string.IsNullOrEmpty(uuid)
                        ? null
                        : uuid.StartsWith("http")
                            ? uuid
                            : $"https://leonardo.osnova.io/{uuid}/-/scale_crop/480x/";

                    attachments.Add(new CommentMediaAttachmentViewItem
                    {
                        Type = item.Type,
                        Url = imageUrl,
                        Title = item.Title,
                        Description = item.Description,
                        Hostname = item.Hostname
                    });
                }
            }

            return attachments;
        }
    }

    public class CommentTextSegment
    {
        public string Text { get; set; }
        public bool IsQuote { get; set; }
    }
}
