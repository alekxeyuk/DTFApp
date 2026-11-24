using DTFApp.Models;
using DTFApp.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DTFApp.Tests
{
    public class FakeDtfApiService : IDtfApiService
    {
        public NewsResult NewsResult { get; set; }
        public EntryResponse EntryResponse { get; set; }
        public CommentsResponse CommentsResponse { get; set; }
        public string LoginResult { get; set; }
        public Exception NewsException { get; set; }
        public Exception EntryException { get; set; }
        public Exception CommentsException { get; set; }
        public Exception LoginException { get; set; }
        public long LastRequestedNewsId { get; private set; }
        public long LastRequestedContentId { get; private set; }
        public long LastRequestedEntryId { get; private set; }

        public Task<NewsResult> GetNewsAsync(long lastId = 0, CancellationToken ct = default)
        {
            LastRequestedNewsId = lastId;
            if (NewsException != null) throw NewsException;
            return Task.FromResult(NewsResult);
        }

        public Task<EntryResponse> GetEntryAsync(long id, CancellationToken ct = default)
        {
            LastRequestedEntryId = id;
            if (EntryException != null) throw EntryException;
            return Task.FromResult(EntryResponse);
        }

        public Task<CommentsResponse> GetCommentsAsync(long contentId, CancellationToken ct = default)
        {
            LastRequestedContentId = contentId;
            if (CommentsException != null) throw CommentsException;
            return Task.FromResult(CommentsResponse);
        }

        public Task<BadgeAssetsResponse> GetBadgeAssetsAsync(CancellationToken ct = default)
        {
            return Task.FromResult(new BadgeAssetsResponse());
        }

        public Task<QuizResultResponse> GetQuizResultsAsync(string hash, CancellationToken ct = default)
        {
            return Task.FromResult(new QuizResultResponse());
        }

        public Task<string> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            if (LoginException != null) throw LoginException;
            return Task.FromResult(LoginResult);
        }

        public Task<string> RegisterAsync(string email, string password, string name, CancellationToken ct = default)
        {
            if (LoginException != null) throw LoginException;
            return Task.FromResult(LoginResult);
        }
    }
}
