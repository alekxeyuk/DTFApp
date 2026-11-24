using DTFApp.Models;
using DTFApp.Services;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace DTFApp.ViewModels
{
    public class EntryViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private EntryResponse _entry;
        private string _errorMessage;

        public EntryResponse Entry
        {
            get => _entry;
            set
            {
                SetProperty(ref _entry, value);
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(CommentsCount));
                OnPropertyChanged(nameof(Url));
            }
        }

        public string Title => Entry?.Result?.Title;

        public Block[] Blocks => Entry?.Result?.Blocks;

        public long CommentsCount => Entry?.Result?.Counters?.Comments ?? 0;

        public string Url => !string.IsNullOrEmpty(Entry?.Result?.Url)
            ? Entry.Result.Url
            : null;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(ErrorVisibility));
                }
            }
        }

        public Visibility ErrorVisibility => string.IsNullOrWhiteSpace(ErrorMessage) ? Visibility.Collapsed : Visibility.Visible;

        public EntryViewModel() : this(AppServices.Api) { }

        public EntryViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadEntryAsync(long id, CancellationToken ct = default)
        {
            ErrorMessage = null;
            try
            {
                Entry = await _apiService.GetEntryAsync(id, ct);
                SaveCache(id, Entry);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var cached = await LoadCacheAsync(id);
                if (cached != null)
                {
                    Entry = cached;
                }
                else
                {
                    ErrorMessage = ex.Message;
                    System.Diagnostics.Debug.WriteLine($"Error loading entry {id}: {ex}");
                }
            }
        }

        private static string CacheFileName(long id) => $"entry_{id}.json";

        private static async void SaveCache(long id, EntryResponse entry)
        {
            try
            {
                if (entry == null) return;
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.CreateFileAsync(CacheFileName(id), CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(entry));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving entry cache: {ex.Message}");
            }
        }

        private static async Task<EntryResponse> LoadCacheAsync(long id)
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.GetFileAsync(CacheFileName(id));
                if (file == null) return null;
                var json = await FileIO.ReadTextAsync(file);
                if (string.IsNullOrWhiteSpace(json)) return null;
                return JsonConvert.DeserializeObject<EntryResponse>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
