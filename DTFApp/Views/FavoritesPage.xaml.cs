using DTFApp.Models;
using DTFApp.Services;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DTFApp.Views
{
    public sealed partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var favorites = FavoritesService.GetAll();
            FavoritesListView.ItemsSource = favorites;
            EmptyTextBlock.Visibility = favorites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void FavoritesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is NewsData newsItem)
            {
                Frame.Navigate(typeof(EntryPage), newsItem.Id);
            }
        }
    }
}
