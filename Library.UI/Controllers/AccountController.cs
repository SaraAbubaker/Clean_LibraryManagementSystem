using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.UserApiDtos.UserDtos;
using Library.UI.Helpers;
using Library.UI.Models.String_constant;
using Library.UI.Services;
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
        private readonly IApiClient _apiClient;
        private readonly ApiSettings _apiSettings;

        public AccountController(
            IApiClient apiClient,
            IOptions<ApiSettings> apiSettings)
        {
            _apiClient = apiClient;
            _apiSettings = apiSettings.Value;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register() => View(new RegisterUserMessage());

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login() => View(new LoginUserMessage());

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserMessage input)
        {
            if (!ModelState.IsValid)
                return View(input);

            try
            {
                // Use the new PostAsync with apiName = "UserApi"
                var registerResponse =
                    await _apiClient.PostAsync<RegisterUserMessage, ApiResponse<LoginUserResponseMessage>>(
                        _apiSettings.UserApi.Endpoints.Register,
                        input,
                        apiName: "UserApi"
                    );

                if (registerResponse == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Authentication service unavailable. Please try again later.");
                    return View(input);
                }

                if (registerResponse.Success &&
                    registerResponse.Data != null &&
                    !string.IsNullOrEmpty(registerResponse.Data.AccessToken))
                {
                    await SignInHelper.SignInWithJwtAsync(HttpContext, registerResponse.Data.AccessToken);
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Index", "Home");
                }

                ApiErrorMapper.MapRegisterErrorToModelState(
                    ModelState, registerResponse.Message, input);

                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(input);
            }
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginUserMessage input)
        {
            if (!ModelState.IsValid)
                return View(input);

            try
            {
                var apiResponse = await _apiClient.PostAsync<LoginUserMessage, ApiResponse<LoginUserResponseMessage>>(
                    _apiSettings.UserApi.Endpoints.Login,
                    input,
                    apiName: "UserApi"
                );

                if (apiResponse == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Authentication service unavailable. Please try again later.");
                    return View(input);
                }

                if (apiResponse.Success && apiResponse.Data != null)
                {
                    await SignInHelper.SignInWithJwtAsync(HttpContext, apiResponse.Data.AccessToken);
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }

                ApiErrorMapper.MapLoginErrorToModelState(ModelState, apiResponse.Message, input);
                return View(input);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unexpected error: {ex.Message}");
                return View(input);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}
