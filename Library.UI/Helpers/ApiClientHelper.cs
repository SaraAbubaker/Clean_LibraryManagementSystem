using System.Text;
using System.Text.Json;
using Library.Common.RabbitMqMessages.ApiResponses;

namespace Library.UI.Helpers
{
    public static class ApiClientHelper
    {
        public static async Task<ApiResponse<T>> PostJsonAsync<T>(
            HttpClient client, string url, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"HTTP {response.StatusCode}"
                };
            }

            var body = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonSerializer.Deserialize<ApiResponse<T>>(body)
                    ?? new ApiResponse<T> { Success = false, Message = "Empty or invalid response" };
            }
            catch (JsonException ex)
            {
                return new ApiResponse<T> { Success = false, Message = $"Invalid JSON: {ex.Message}" };
            }
        }
    }
}