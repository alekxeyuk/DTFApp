using Windows.UI.Xaml;

namespace DTFApp.ViewModels
{
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
