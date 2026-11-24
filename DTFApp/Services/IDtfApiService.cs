using DTFApp.Models;
using System.Threading;
using System.Threading.Tasks;

namespace DTFApp.Services
{
    public interface IDtfApiService
    {
        Task<NewsResult> GetNewsAsync(long lastId = 0, CancellationToken ct = default);
        Task<EntryResponse> GetEntryAsync(long id, CancellationToken ct = default);
        Task<CommentsResponse> GetCommentsAsync(long contentId, CancellationToken ct = default);
        Task<BadgeAssetsResponse> GetBadgeAssetsAsync(CancellationToken ct = default);
        Task<QuizResultResponse> GetQuizResultsAsync(string hash, CancellationToken ct = default);
        Task<string> LoginAsync(string email, string password, CancellationToken ct = default);
        Task<string> RegisterAsync(string email, string password, string name, CancellationToken ct = default);
        Task<string> AddCommentAsync(long contentId, string text, long replyTo, string token, CancellationToken ct = default);
    }
}
