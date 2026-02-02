using Library.Common.DTOs.ApiResponseDtos;
using System.Text;
using System.Text.Json;

namespace Library.UI.Helpers
{
    public static class ApiClientHelper
    {
        public static async Task<ApiResponse<T>> PostJsonAsync<T>(HttpClient client, string url, object payload)
        {

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(url, content);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Data = default,
                    Message = $"Network error: {ex.Message}"
                };
            }

            var body = await response.Content.ReadAsStringAsync();

            // Deserialize API response, fallback to default ApiResponse if null
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(body,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                         ?? new ApiResponse<T>
                         {
                             Success = false,
                             Data = default,
                             Message = $"Invalid server response: {response.StatusCode}"
                         };

            // If HTTP request failed but message is empty, mark as failed
            if (!response.IsSuccessStatusCode && string.IsNullOrEmpty(result.Message))
            {
                result.Success = false;
                result.Message = $"Request failed: {response.StatusCode}";
            }

            return result;
        }
    }
}
