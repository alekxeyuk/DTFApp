using DTFApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace DTFApp.Services
{
    public static class BadgeCacheService
    {
        private const string CacheFileName = "badge-cache.json";
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, string> _badgeStaticUuids = new Dictionary<string, string>();
        private static readonly IDtfApiService _apiService = new DtfApiService();
        private static Task _loadTask;
        private static Task _updateTask;
        private static bool _hasUpdatedBadges;

        public static Task EnsureCacheLoadedAsync()
        {
            lock (_lock)
            {
                if (_loadTask == null)
                {
                    _loadTask = LoadCacheAsync();
                }

                return _loadTask;
            }
        }

        public static Task UpdateBadgesAsync(CancellationToken ct = default)
        {
            lock (_lock)
            {
                if (_hasUpdatedBadges)
                {
                    return EnsureCacheLoadedAsync();
                }

                if (_updateTask == null)
                {
                    _updateTask = UpdateBadgesCoreAsync();
                }

                return _updateTask;
            }
        }

        public static string GetBadgeUrl(string badgeId)
        {
            if (string.IsNullOrWhiteSpace(badgeId)) return null;

            string staticUuid;
            lock (_lock)
            {
                if (!_badgeStaticUuids.TryGetValue(badgeId, out staticUuid)) return null;
            }

            if (string.IsNullOrWhiteSpace(staticUuid)) return null;
            return $"https://leonardo.osnova.io/{staticUuid}/-/scale_crop/18x/";
        }

        private static async Task UpdateBadgesCoreAsync()
        {
            try
            {
                await EnsureCacheLoadedAsync();

                var response = await _apiService.GetBadgeAssetsAsync();
                var badges = response?.Result?.Badges;
                if (badges == null) return;

                var changed = false;
                lock (_lock)
                {
                    foreach (var badge in badges)
                    {
                        if (string.IsNullOrWhiteSpace(badge?.Id) || string.IsNullOrWhiteSpace(badge.StaticUuid)) continue;

                        if (!_badgeStaticUuids.TryGetValue(badge.Id, out string existingUuid) || existingUuid != badge.StaticUuid)
                        {
                            _badgeStaticUuids[badge.Id] = badge.StaticUuid;
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    await SaveCacheAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating badge cache: {ex.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _hasUpdatedBadges = true;
                }
            }
        }

        private static async Task LoadCacheAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.GetFileAsync(CacheFileName);
                var json = await FileIO.ReadTextAsync(file);
                var cachedBadges = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (cachedBadges == null) return;

                lock (_lock)
                {
                    _badgeStaticUuids.Clear();
                    foreach (var badge in cachedBadges)
                    {
                        if (string.IsNullOrWhiteSpace(badge.Key) || string.IsNullOrWhiteSpace(badge.Value)) continue;
                        _badgeStaticUuids[badge.Key] = badge.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading badge cache: {ex.Message}");
            }
        }

        private static async Task SaveCacheAsync()
        {
            Dictionary<string, string> badgesToSave;
            lock (_lock)
            {
                badgesToSave = new Dictionary<string, string>(_badgeStaticUuids);
            }

            var folder = ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(CacheFileName, CreationCollisionOption.ReplaceExisting);
            var json = JsonConvert.SerializeObject(badgesToSave);
            await FileIO.WriteTextAsync(file, json);
        }
    }
}
