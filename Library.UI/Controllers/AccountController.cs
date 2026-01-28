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

            try
            {
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
                        TempData["SuccessMessage"] = "Registration successful!";
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Inline feedback for expected failure
                TempData["ErrorMessage"] = registerResponse?.Message ?? "Registration failed.";
                return View(input);
            }
            catch (Exception ex)
            {
                // Critical failure → Error view
                ModelState.AddModelError("", ex.Message);
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Library.UserApi");

                var loginResponse = await ApiClientHelper.PostJsonAsync<LoginUserResponseMessage>(
                    client, _apiSettings.Endpoints.Login, input);

                if (loginResponse?.Success == true && loginResponse.Data != null)
                {
                    await SignInHelper.SignInWithJwtAsync(HttpContext, loginResponse.Data.AccessToken);
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = loginResponse?.Message ?? "Login failed.";
                return View(input);
            }
            catch (Exception ex)
            {
                // Critical failure → Error view
                ModelState.AddModelError("", ex.Message);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out.";
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
