using DTFApp.Helpers;
using DTFApp.Models;
using DTFApp.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DTFApp.ViewModels
{
    public class CommentsViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private bool _isLoading;
        private string _errorMessage;
        private bool _hasLoadedComments;

        public ObservableCollection<CommentViewItem> Comments { get; } = new ObservableCollection<CommentViewItem>();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                    OnPropertyChanged(nameof(EmptyVisibility));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(ErrorVisibility));
                    OnPropertyChanged(nameof(EmptyVisibility));
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ErrorVisibility => string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility EmptyVisibility => _hasLoadedComments && !IsLoading && string.IsNullOrWhiteSpace(ErrorMessage) && Comments.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        public CommentsViewModel() : this(new DtfApiService()) { }

        public CommentsViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadCommentsAsync(long contentId)
        {
            if (_isLoading) return;
            IsLoading = true;
            ErrorMessage = null;

            try
            {
                Comments.Clear();
                _hasLoadedComments = false;
                OnPropertyChanged(nameof(EmptyVisibility));

                await BadgeCacheService.UpdateBadgesAsync();

                var response = await _apiService.GetCommentsAsync(contentId);
                if (response?.Result?.Items == null) return;

                var byId = new Dictionary<long, CommentViewItem>();
                var ordered = new List<CommentViewItem>();
                foreach (var comment in response.Result.Items)
                {
                    var item = new CommentViewItem(comment);
                    Comments.Add(item);
                    ordered.Add(item);
                    byId[item.Id] = item;
                }

                BuildCommentTree(ordered, byId);
                var roots = new List<CommentViewItem>();
                foreach (var item in ordered)
                {
                    var replyTo = item.Comment?.ReplyTo ?? 0;
                    if (replyTo == 0 || !byId.ContainsKey(replyTo))
                    {
                        roots.Add(item);
                    }
                }
                foreach (var root in roots)
                    ItemVisibility(root, parentHidden: false);

                OnPropertyChanged(nameof(EmptyVisibility));
            }
            finally
            {
                _hasLoadedComments = true;
                IsLoading = false;
            }
        }

        private static void BuildCommentTree(List<CommentViewItem> ordered, Dictionary<long, CommentViewItem> byId)
        {
            foreach (var item in ordered)
            {
                var replyTo = item.Comment?.ReplyTo ?? 0;
                if (replyTo > 0 && byId.TryGetValue(replyTo, out var parent))
                {
                    parent.Children.Add(item);
                    parent.HasReplies = true;
                }
            }
        }

        public void ToggleCollapse(CommentViewItem item)
        {
            if (item == null) return;
            item.IsExpanded = !item.IsExpanded;
            ItemVisibility(item, parentHidden: false);
        }

        private void ItemVisibility(CommentViewItem item, bool parentHidden)
        {
            if (item.IsHiddenByCollapse && parentHidden) return;

            item.IsHiddenByCollapse = parentHidden;
            var myHidden = parentHidden || (!item.IsExpanded && item.Children.Count > 0);

            foreach (var child in item.Children)
            {
                ItemVisibility(child, myHidden);
            }
        }
    }

    public class CommentViewItem : BaseViewModel
    {
        private const int MaxIndentLevel = 5;
        private readonly string _text;
        private readonly Thickness _indentMargin;
        private bool _isExpanded = true;
        private bool _hasReplies;
        private bool _isHiddenByCollapse;

        public CommentViewItem(Comment comment)
        {
            Comment = comment;
            MediaAttachments = CreateMediaAttachments(comment?.Media);
            _text = WebUtility.HtmlDecode(HtmlHelper.StripHtml(comment?.Text ?? ""));

            var level = comment?.Level ?? 0;
            if (level < 0) level = 0;
            if (level > MaxIndentLevel) level = MaxIndentLevel;
            _indentMargin = new Thickness(level * 10, level == 0 ? 12 : 0, 0, 0);
        }

        public Comment Comment { get; }
        public ObservableCollection<CommentMediaAttachmentViewItem> MediaAttachments { get; }
        public List<CommentViewItem> Children { get; } = new List<CommentViewItem>();

        public long Id => Comment?.Id ?? 0;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value))
                {
                    OnPropertyChanged(nameof(ExpandIconGlyph));
                }
            }
        }

        public bool HasReplies
        {
            get => _hasReplies;
            set
            {
                if (_hasReplies != value)
                {
                    _hasReplies = value;
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

        public string ExpandIconGlyph => _isExpanded ? "\xE70D" : "\xE76C";

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
        public string Text => _text;

        public Visibility TextVisibility => string.IsNullOrWhiteSpace(Text) ? Visibility.Collapsed : Visibility.Visible;

        public Thickness IndentMargin => _indentMargin;

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

        private static ObservableCollection<CommentMediaAttachmentViewItem> CreateMediaAttachments(JArray media)
        {
            var attachments = new ObservableCollection<CommentMediaAttachmentViewItem>();
            if (media == null) return attachments;

            foreach (var token in media)
            {
                if (!(token is JObject obj)) continue;

                var type = (string)obj["type"];
                if (!(obj["data"] is JObject data)) continue;

                if (type == "image" || type == "movie")
                {
                    var uuid = (string)data["uuid"];
                    if (string.IsNullOrEmpty(uuid)) continue;

                    var url = uuid.StartsWith("http")
                        ? uuid
                        : $"https://leonardo.osnova.io/{uuid}/-/scale_crop/480x/";

                    attachments.Add(new CommentMediaAttachmentViewItem
                    {
                        Type = type,
                        Url = url
                    });
                }
                else if (type == "link")
                {
                    var image = data["image"] as JObject;
                    var uuid = !(image?["data"] is JObject imageData) ? null : (string)imageData["uuid"];
                    var imageUrl = string.IsNullOrEmpty(uuid)
                        ? null
                        : uuid.StartsWith("http")
                            ? uuid
                            : $"https://leonardo.osnova.io/{uuid}/-/scale_crop/480x/";

                    attachments.Add(new CommentMediaAttachmentViewItem
                    {
                        Type = type,
                        Url = imageUrl,
                        Title = WebUtility.HtmlDecode((string)data["title"]),
                        Description = WebUtility.HtmlDecode((string)data["description"]),
                        Hostname = WebUtility.HtmlDecode((string)data["hostname"])
                    });
                }
            }

            return attachments;
        }
    }

    public class CommentMediaAttachmentViewItem
    {
        public string Type { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Hostname { get; set; }
        public bool IsLink => Type == "link";
        public Visibility TitleVisibility => string.IsNullOrWhiteSpace(Title) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility HostnameVisibility => string.IsNullOrWhiteSpace(Hostname) ? Visibility.Collapsed : Visibility.Visible;
    }
}
