using DTFApp.Models;
using DTFApp.Services;
using System.Threading.Tasks;

namespace DTFApp.ViewModels
{
    public class EntryViewModel : BaseViewModel
    {
        private readonly IDtfApiService _apiService;
        private EntryResponse _entry;

        public EntryResponse Entry
        {
            get => _entry;
            set
            {
                SetProperty(ref _entry, value);
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Title => Entry?.Result?.Title;

        public Block[] Blocks => Entry?.Result?.Blocks;

        public EntryViewModel() : this(new DtfApiService()) { }

        public EntryViewModel(IDtfApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task LoadEntryAsync(long id)
        {
            Entry = await _apiService.GetEntryAsync(id);
        }
    }
}
