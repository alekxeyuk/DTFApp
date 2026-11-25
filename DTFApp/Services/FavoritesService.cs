using DTFApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.Storage;

namespace DTFApp.Services
{
    public static class FavoritesService
    {
        private const string FavoritesKey = "favorites";

        public static List<NewsData> GetAll()
        {
            object stored;
            var json = ApplicationData.Current.LocalSettings.Values.TryGetValue(FavoritesKey, out stored) ? stored as string : null;
            if (string.IsNullOrEmpty(json)) return new List<NewsData>();
            return JsonConvert.DeserializeObject<List<NewsData>>(json) ?? new List<NewsData>();
        }

        public static bool IsFavorite(long id)
        {
            return GetAll().Exists(n => n.Id == id);
        }

        public static bool Toggle(NewsData item)
        {
            var all = GetAll();
            var index = all.FindIndex(n => n.Id == item.Id);
            bool nowFavorite;
            if (index >= 0)
            {
                all.RemoveAt(index);
                nowFavorite = false;
            }
            else
            {
                all.Add(item);
                nowFavorite = true;
            }
            ApplicationData.Current.LocalSettings.Values[FavoritesKey] = JsonConvert.SerializeObject(all);
            return nowFavorite;
        }
    }
}
