using DTFApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DTFApp.Services
{
    public class DtfApiService : IDtfApiService
    {
        private static readonly HttpClient _sharedHttpClient = CreateDefaultClient();

        private readonly HttpClient _httpClient;

        public DtfApiService() : this(_sharedHttpClient) { }

        public DtfApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static HttpClient CreateDefaultClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DTFApp");
            return client;
        }

        public async Task<NewsResult> GetNewsAsync(long lastId = 0, CancellationToken ct = default)
        {
            string url = lastId == 0
                ? $"{ApiConstants.ApiBaseUrl}/v2.0/news"
                : $"{ApiConstants.ApiBaseUrl}/v2.0/news?lastId={lastId}";

            var response = await GetStringAsync(url, ct);
            var newsResponse = JsonConvert.DeserializeObject<NewsResponse>(response);
            return newsResponse?.Result;
        }

        public async Task<EntryResponse> GetEntryAsync(long id, CancellationToken ct = default)
        {
            var response = await GetStringAsync($"{ApiConstants.ApiBaseUrl}/v2.0/content?id={id}", ct);
            return JsonConvert.DeserializeObject<EntryResponse>(response);
        }

        public async Task<CommentsResponse> GetCommentsAsync(long contentId, CancellationToken ct = default)
        {
            var response = await GetStringAsync($"{ApiConstants.ApiBaseUrl}/v2.0/comments?contentId={contentId}", ct);
            return JsonConvert.DeserializeObject<CommentsResponse>(response);
        }

        public async Task<BadgeAssetsResponse> GetBadgeAssetsAsync(CancellationToken ct = default)
        {
            var response = await GetStringAsync($"{ApiConstants.ApiBaseUrl}/v2.0/assets?type=badges", ct);
            return JsonConvert.DeserializeObject<BadgeAssetsResponse>(response);
        }

        public async Task<QuizResultResponse> GetQuizResultsAsync(string hash, CancellationToken ct = default)
        {
            var response = await GetStringAsync($"{ApiConstants.ApiBaseUrl}/v2.0/quiz/{hash}/results", ct);
            return JsonConvert.DeserializeObject<QuizResultResponse>(response);
        }

        public async Task<string> LoginAsync(string email, string password, CancellationToken ct = default)
        {
            var form = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            };
            return await PostStringAsync($"{ApiConstants.ApiBaseUrl}/v3.0/auth/email/login", form, ct);
        }

        public async Task<string> RegisterAsync(string email, string password, string name, CancellationToken ct = default)
        {
            var form = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "name", name }
            };
            return await PostStringAsync($"{ApiConstants.ApiBaseUrl}/v3.0/auth/email/register", form, ct);
        }

        public async Task<string> AddCommentAsync(long contentId, string text, long replyTo, string token, CancellationToken ct = default)
        {
            var payload = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { "contentId", contentId },
                { "text", text },
                { "replyTo", replyTo }
            });
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiConstants.ApiBaseUrl}/v2.0/comments/add"))
            {
                request.Content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                using (var response = await _httpClient.SendAsync(request, ct))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        private async Task<string> GetStringAsync(string url, CancellationToken ct)
        {
            using (var response = await _httpClient.GetAsync(url, ct))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> PostStringAsync(string url, Dictionary<string, string> form, CancellationToken ct)
        {
            using (var response = await _httpClient.PostAsync(url, new FormUrlEncodedContent(form), ct))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
