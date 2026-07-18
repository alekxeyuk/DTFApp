using DTFApp.Models;
using DTFApp.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
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

        public async Task LoadCommentsAsync(long contentId, CancellationToken ct = default)
        {
            if (_isLoading) return;

            IsLoading = true;
            ErrorMessage = null;
            Comments.Clear();
            _hasLoadedComments = false;
            OnPropertyChanged(nameof(EmptyVisibility));

            try
            {
                await BadgeCacheService.UpdateBadgesAsync(ct);

                var response = await _apiService.GetCommentsAsync(contentId, ct);
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
}
