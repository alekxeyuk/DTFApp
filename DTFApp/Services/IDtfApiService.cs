using DTFApp.Models;
using System.Threading.Tasks;

namespace DTFApp.Services
{
    public interface IDtfApiService
    {
        Task<NewsResult> GetNewsAsync(long lastId = 0);
        Task<EntryResponse> GetEntryAsync(long id);
        Task<QuizResultResponse> GetQuizResultsAsync(string hash);
        Task<string> LoginAsync(string email, string password);
        Task<string> RegisterAsync(string email, string password, string name);
    }
}
