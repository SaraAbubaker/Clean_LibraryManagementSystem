using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Library.Common.RabbitMqMessages.UserMessages;
using Library.Shared.DTOs.ApiResponses;

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

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            var client = _httpClientFactory.CreateClient("LibraryApi");

            var json = JsonSerializer.Serialize(input);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/users/login", content);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();

                // Deserialize your ApiResponse wrapper
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginUserResponseMessage>>(body);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    HttpContext.Session.SetString("AccessToken", apiResponse.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", apiResponse.Data.RefreshToken);

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(input);
        }
    }
}