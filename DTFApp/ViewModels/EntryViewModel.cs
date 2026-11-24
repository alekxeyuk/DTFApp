using DTFApp.Models;
using DTFApp.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
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
            }
        }

        public string Title => Entry?.Result?.Title;

        public Block[] Blocks => Entry?.Result?.Blocks;

        public long CommentsCount => Entry?.Result?.Counters?.Comments ?? 0;

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
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error loading entry {id}: {ex}");
            }
        }
    }
}
