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

namespace Library.UI.Controllers.UserControllers
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
        public IActionResult Register() => View();

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
                // Call register endpoint via generic API client (expect ApiResponse to get error message)
                var registerResponse = await _apiClient.PostAsync<RegisterUserMessage, ApiResponse<UserListMessage>>(
                    _apiSettings.Endpoints.Register, input);

                if (registerResponse == null)
                {
                    ViewBag.LoginError = "Authentication service unavailable. Please try again later.";
                    return View(input);
                }

                if (registerResponse.Success && registerResponse.Data != null && !string.IsNullOrEmpty(registerResponse.Data.Token))
                {
                    // Automatically log in using the returned token
                    await SignInHelper.SignInWithJwtAsync(HttpContext, registerResponse.Data.Token);
                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Index", "Home");
                }

                // Map API-provided error message to specific fields like Login does
                ApiErrorMapper.MapRegisterErrorToModelState(ModelState, registerResponse.Message, input);

                return View(input);
            }
            catch (Exception ex)
            {
                ViewBag.LoginError = ex.Message;
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
                    _apiSettings.Endpoints.Login, input);

                // Defensive: PostAsync can return null on transport/deserialization failure.
                if (apiResponse == null)
                {
                    ModelState.AddModelError(string.Empty, "Authentication service unavailable. Please try again later.");
                    return View(input);
                }

                if (apiResponse.Success && apiResponse.Data != null)
                {
                    // Successful login — sign in with JWT
                    await SignInHelper.SignInWithJwtAsync(HttpContext, apiResponse.Data.AccessToken);
                    TempData["SuccessMessage"] = "Login successful!";
                    return RedirectToAction("Index", "Home");
                }

                // Failed login — map API error message to correct field
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