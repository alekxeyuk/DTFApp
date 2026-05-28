using DTFApp.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DTFApp.Views
{
    public sealed partial class CommentsPage : Page
    {
        private long _contentId;

        public CommentsViewModel ViewModel { get; } = new CommentsViewModel();

        public CommentsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is long id)
            {
                _contentId = id;
                LoadCommentsAsync(id);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private async void LoadCommentsAsync(long contentId)
        {
            try
            {
                await ViewModel.LoadCommentsAsync(contentId);
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error loading comments for {_contentId}: {ex.Message}");
            }
        }
    }
}
