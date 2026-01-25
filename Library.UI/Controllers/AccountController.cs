using Library.Common.RabbitMqMessages.UserMessages;
using Library.UI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            if (!ModelState.IsValid) return View(input);

            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var registerResponse = await ApiClientHelper.PostJsonAsync<UserListMessage>(
                client, "/api/user/register", input);

            if (registerResponse?.Success == true)
            {
                var loginResponse = await ApiClientHelper.PostJsonAsync<LoginUserResponseMessage>(
                    client, "/api/user/login",
                    new LoginUserMessage { UsernameOrEmail = input.Email, Password = input.Password });

                if (loginResponse?.Success == true && loginResponse.Data != null)
                {
                    // ✅ Use SignInHelper to create cookie from JWT
                    await SignInHelper.SignInWithJwtAsync(HttpContext, loginResponse.Data.AccessToken);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewData["ErrorMessage"] = registerResponse?.Message ?? "Registration failed.";
            return View(input);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            var client = _httpClientFactory.CreateClient("Library.UserApi");

            var loginResponse = await ApiClientHelper.PostJsonAsync<LoginUserResponseMessage>(
                client, "/api/user/login", input);

            if (loginResponse?.Success == true && loginResponse.Data != null)
            {
                // ✅ Use SignInHelper to create cookie from JWT
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