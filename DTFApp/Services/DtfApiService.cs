using DTFApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DTFApp.Services
{
    public class DtfApiService : IDtfApiService
    {
        private readonly HttpClient _httpClient;

        public DtfApiService()
        {
            _httpClient = new HttpClient();
        }

        public DtfApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NewsResult> GetNewsAsync(long lastId = 0)
        {
            string url = lastId == 0
                ? "https://api.dtf.ru/v2.0/news"
                : $"https://api.dtf.ru/v2.0/news?lastId={lastId}";

            var response = await _httpClient.GetStringAsync(url);
            var newsResponse = JsonConvert.DeserializeObject<NewsResponse>(response);
            return newsResponse?.Result;
        }

        public async Task<EntryResponse> GetEntryAsync(long id)
        {
            var response = await _httpClient.GetStringAsync($"https://api.dtf.ru/v2.0/content?id={id}");
            return JsonConvert.DeserializeObject<EntryResponse>(response);
        }

        public async Task<QuizResultResponse> GetQuizResultsAsync(string hash)
        {
            var response = await _httpClient.GetStringAsync($"https://api.dtf.ru/v2.0/quiz/{hash}/results");
            return JsonConvert.DeserializeObject<QuizResultResponse>(response);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var postDict = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password }
            };
            var response = await _httpClient.PostAsync(
                "https://api.dtf.ru/v3.0/auth/email/login",
                new FormUrlEncodedContent(postDict));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> RegisterAsync(string email, string password, string name)
        {
            var postDict = new Dictionary<string, string>
            {
                { "email", email },
                { "password", password },
                { "name", name }
            };
            var response = await _httpClient.PostAsync(
                "https://api.dtf.ru/v3.0/auth/email/register",
                new FormUrlEncodedContent(postDict));
            return await response.Content.ReadAsStringAsync();
        }
    }
}
