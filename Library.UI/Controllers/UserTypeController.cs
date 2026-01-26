using Library.Common.RabbitMqMessages.ApiResponses;
using Library.Common.RabbitMqMessages.UserTypeMessages;
using Library.UI.Models;
using Library.UI.Models.String_constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Library.UI.Controllers
{
    [Authorize(Policy = "usertype.manage")]
    public class UserTypeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public UserTypeController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new UserTypeViewModel();

            try
            {
                var token = User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                if (string.IsNullOrEmpty(token))
                {
                    model.ErrorMessage = "No JWT found in claims.";
                    return View(model);
                }

                var client = _httpClientFactory.CreateClient("Library.UserApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ✅ Use endpoint from appsettings.json
                var response = await client.GetAsync(_apiSettings.Endpoints.UserTypes);
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = $"Failed to fetch user types: {response.StatusCode}";
                    return View(model);
                }

                var body = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserTypeListMessage>>>(body);

                model.UserTypes = apiResponse?.Data ?? new List<UserTypeListMessage>();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Unexpected error: {ex.Message}";
            }

            return View(model);
        }
    }
}
