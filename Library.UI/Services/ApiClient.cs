using Library.UI.Helpers;
using Library.UI.Models.String_constant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net;

namespace Library.UI.Services
{
    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApiSettings _apiSettings;

        public ApiClient(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiSettings = apiSettings.Value;
        }

        // Core helpers

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("Library.UserApi");

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

        // Public API

        public async Task<T?> GetQueryAsync<T>(string basePath)
        {
            var client = CreateClient();

            var response = await client.GetAsync(
                ApiUrlBuilder.ForQuery(basePath)
            );

            response.EnsureSuccessStatusCode();

            return await DeserializeAsync<T>(response);
        }

        public async Task<T?> GetByIdAsync<T>(string basePath, int id, int? userId = null)
        {
            var client = CreateClient();

            var response = await client.GetAsync(
                ApiUrlBuilder.ForId(basePath, id, userId)
            );

            response.EnsureSuccessStatusCode();

            return await DeserializeAsync<T>(response);
        }

        public async Task<HttpResponseMessage> PostAsync<TBody>(string basePath, TBody body, int? userId = null)
        {
            var client = CreateClient();

            var url = userId.HasValue
                ? $"{basePath}?createdByUserId={userId}"
                : basePath;

            return await client.PostAsync(url, JsonContent.Create(body));
        }

        public async Task<HttpResponseMessage> PutAsync<TBody>(string basePath, int id, TBody body, int? userId = null)
        {
            var client = CreateClient();

            var url = ApiUrlBuilder.ForId(basePath, id, userId);

            return await client.PutAsync(url, JsonContent.Create(body));
        }

        public async Task<HttpResponseMessage> PutArchiveAsync(string basePath, int id, int userId)
        {
            var client = CreateClient();

            return await client.PutAsync(
                $"{basePath}/archive/{id}?userId={userId}",
                null
            );
        }

        public async Task<T> PostAsync<TBody, T>(string basePath, TBody body, int? userId = null)
        {
            var response = await PostAsync(basePath, body, userId);

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Attempt to deserialize regardless of status code; some APIs return structured error payloads
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<T>(content, options);
                    if (parsed != null)
                        return parsed;
                }
                catch (JsonException)
                {
                    // fall through to throw a richer exception below
                }
            }

            // If we get here, either content was empty, deserialization failed, or parsed was null.
            if (response.IsSuccessStatusCode)
            {
                // success but nothing to deserialize -> treat as unexpected
                throw new ApiClientException($"Empty or invalid response from API for endpoint '{basePath}'.", (HttpStatusCode?)response.StatusCode, content);
            }
            else
            {
                // non-successful status code -> throw with details (caller can catch and map to ModelState)
                throw new ApiClientException($"API returned error {(int)response.StatusCode} ({response.ReasonPhrase}).", (HttpStatusCode?)response.StatusCode, content);
            }
        }

    }
}
