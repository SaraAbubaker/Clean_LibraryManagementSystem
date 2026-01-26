using Library.Common.RabbitMqMessages.UserMessages;
using Library.UI.Helpers;
using Library.UI.Models.String_constant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Library.UI.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApiSettings _apiSettings;

        public AccountController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiSettings = apiSettings.Value;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Login() => View();


        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserMessage input)
        {
            if (!ModelState.IsValid) return View(input);

            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var registerResponse = await ApiClientHelper.PostJsonAsync<UserListMessage>(
                client, _apiSettings.Endpoints.Register, input);

            if (registerResponse?.Success == true)
            {
                var loginResponse = await ApiClientHelper.PostJsonAsync<LoginUserResponseMessage>(
                    client, _apiSettings.Endpoints.Login,
                    new LoginUserMessage { UsernameOrEmail = input.Email, Password = input.Password });

                if (loginResponse?.Success == true && loginResponse.Data != null)
                {
                    await SignInHelper.SignInWithJwtAsync(HttpContext, loginResponse.Data.AccessToken);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewData["ErrorMessage"] = registerResponse?.Message ?? "Registration failed.";
            return View(input);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var loginResponse = await ApiClientHelper.PostJsonAsync<LoginUserResponseMessage>(
                client, _apiSettings.Endpoints.Login, input);

            if (loginResponse?.Success == true && loginResponse.Data != null)
            {
                await SignInHelper.SignInWithJwtAsync(HttpContext, loginResponse.Data.AccessToken);
                return RedirectToAction("Index", "Home");
            }

            ViewData["ErrorMessage"] = loginResponse?.Message ?? "Login failed.";
            return View(input);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
