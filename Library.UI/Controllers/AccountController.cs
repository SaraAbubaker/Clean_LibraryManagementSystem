using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Library.Common.RabbitMqMessages.UserMessages;
using Library.Shared.DTOs.ApiResponses;
using Microsoft.AspNetCore.Authorization;

namespace Library.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login() => View();


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var json = JsonSerializer.Serialize(input);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/user/login", content);

            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Deserialize your ApiResponse wrapper
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginUserResponseMessage>>(body);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    HttpContext.Session.SetString("AccessToken", apiResponse.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", apiResponse.Data.RefreshToken);

                    return RedirectToAction("Index", "Home");
                }

                // If API responded but failed, show its message
                if (!string.IsNullOrEmpty(apiResponse?.Message))
                {
                    ViewData["ErrorMessage"] = apiResponse.Message;
                }
                else
                {
                    ViewData["ErrorMessage"] = "Invalid login attempt";
                }
            }
            else
            {
                // Try to deserialize error response from API
                var apiError = JsonSerializer.Deserialize<ApiResponse<LoginUserMessage>>(body);

                if (!string.IsNullOrEmpty(apiError?.Message))
                {
                    ViewData["ErrorMessage"] = apiError.Message; // e.g. "Invalid password"
                }
                else
                {
                    ViewData["ErrorMessage"] = $"Login failed: {response.StatusCode}";
                }
            }

            // Return view with input so fields stay filled
            return View(input);
        }

    }
}