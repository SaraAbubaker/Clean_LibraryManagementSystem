using Library.UI.Helpers;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;

namespace Library.UI.Services
{
    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        // ================== Core helper ==================
        private HttpClient CreateClient(string apiName = "LibraryApi")
        {
            string clientName = apiName switch
            {
                "UserApi" => "Library.UserApi",
                _ => "Library.LibraryApi"   // default to LibraryApi
            };

            var client = _httpClientFactory.CreateClient(clientName);

            // Add JWT token if available
            var token = _httpContextAccessor.HttpContext?
                .User?
                .Claims?
                .FirstOrDefault(c => c.Type == "access_token")?
                .Value;

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return default;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<T>(json, options);
        }

        // ================== Public API ==================

        public async Task<T?> GetAsync<T>(string path, string apiName = "LibraryApi")
        {
            var client = CreateClient(apiName);
            var response = await client.GetAsync(path);
            response.EnsureSuccessStatusCode();
            return await DeserializeAsync<T>(response);
        }

        public async Task<T?> GetQueryAsync<T>(string basePath, string apiName = "LibraryApi")
        {
            var client = CreateClient(apiName);
            var response = await client.GetAsync(ApiUrlBuilder.ForQuery(basePath));
            response.EnsureSuccessStatusCode();
            return await DeserializeAsync<T>(response);
        }

        public async Task<T?> GetByIdAsync<T>(string basePath, int id, int? userId = null, string apiName = "LibraryApi")
        {
            var client = CreateClient(apiName);
            var response = await client.GetAsync(ApiUrlBuilder.ForId(basePath, id, userId));
            response.EnsureSuccessStatusCode();
            return await DeserializeAsync<T>(response);
        }

        public async Task<HttpResponseMessage> PostAsync<TBody>(string basePath, TBody body, int? userId = null, string apiName = "LibraryApi")
        {
            var client = CreateClient(apiName);
            var url = userId.HasValue ? $"{basePath}?createdByUserId={userId}" : basePath;
            return await client.PostAsync(url, JsonContent.Create(body));
        }

        public async Task<HttpResponseMessage> PutAsync<TBody>(string basePath, int id, TBody body, int? userId = null, string apiName = "LibraryApi")
        {
            var client = CreateClient(apiName);
            var url = ApiUrlBuilder.ForId(basePath, id, userId);
            return await client.PutAsync(url, JsonContent.Create(body));
        }

        public async Task<HttpResponseMessage> PutArchiveAsync(string basePath, int id, int userId, string apiName = "LibraryApi")
        {
            var client = CreateClient(apiName);
            return await client.PutAsync($"{basePath}/archive/{id}?userId={userId}", null);
        }

        public async Task<T> PostAsync<TBody, T>(string basePath, TBody body, int? userId = null, string apiName = "LibraryApi")
        {
            var response = await PostAsync(basePath, body, userId, apiName);
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<T>(content, options);
                    if (parsed != null)
                        return parsed;
                }
                catch (JsonException) { }
            }

            if (response.IsSuccessStatusCode)
                throw new ApiClientException($"Empty or invalid response from API for endpoint '{basePath}'.", (HttpStatusCode?)response.StatusCode, content);
            else
                throw new ApiClientException($"API returned error {(int)response.StatusCode} ({response.ReasonPhrase}).", (HttpStatusCode?)response.StatusCode, content);
        }
    }
}