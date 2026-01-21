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
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login() => View();


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserMessage input)
        {
            if (!ModelState.IsValid)
                return View(input);

            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var json = JsonSerializer.Serialize(input);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/user/register", content);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserListMessage>>(body);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    //login after successful registration
                    var loginDto = new LoginUserMessage
                    {
                        UsernameOrEmail = input.Email,
                        Password = input.Password
                    };

                    var loginJson = JsonSerializer.Serialize(loginDto);
                    var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

                    var loginResponse = await client.PostAsync("/api/user/login", loginContent);
                    var loginBody = await loginResponse.Content.ReadAsStringAsync();

                    if (loginResponse.IsSuccessStatusCode)
                    {
                        var loginApiResponse = JsonSerializer.Deserialize<ApiResponse<LoginUserResponseMessage>>(loginBody);

                        if (loginApiResponse?.Success == true && loginApiResponse.Data != null)
                        {
                            HttpContext.Session.SetString("AccessToken", loginApiResponse.Data.AccessToken);
                            HttpContext.Session.SetString("RefreshToken", loginApiResponse.Data.RefreshToken);

                            return RedirectToAction("Index", "Home"); // Welcome page
                        }
                    }
                }

                ViewData["ErrorMessage"] = apiResponse?.Message ?? "Registration failed.";
            }
            else
            {
                var apiError = JsonSerializer.Deserialize<ApiResponse<RegisterUserMessage>>(body);
                ViewData["ErrorMessage"] = apiError?.Message ?? $"Registration failed: {response.StatusCode}";
            }

            return View(input);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var json = JsonSerializer.Serialize(input);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/user/login", content);
            var body = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                ViewData["ErrorMessage"] = "Empty response from server.";
                return View(input);
            }

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginUserResponseMessage>>(body);

            if (response.IsSuccessStatusCode && apiResponse?.Success == true && apiResponse.Data != null)
            {
                HttpContext.Session.SetString("AccessToken", apiResponse.Data.AccessToken);
                HttpContext.Session.SetString("RefreshToken", apiResponse.Data.RefreshToken);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ErrorMessage"] = apiResponse?.Message ?? $"Login failed: {response.StatusCode}";
            return View(input);
        }
    }
}