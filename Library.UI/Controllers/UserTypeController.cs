using Library.Common.RabbitMqMessages.ApiResponses;
using Library.Common.RabbitMqMessages.UserTypeMessages;
using Library.Common.StringConstants;
using Library.UI.Helpers;
using Library.UI.Models;
using Library.UI.Models.String_constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Library.UI.Controllers
{
    [Authorize(Policy = PermissionNames.UserManage)]
    public class UserTypeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserTypeController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetAccessToken() =>
            User.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;

        // GET: /UserType → fetch all user types
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new UserTypeViewModel();
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                    return View("Error", model);

                var client = _httpClientFactory.CreateClient("Library.UserApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/usertype/query");
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = $"Failed to fetch user types: {response.StatusCode}";
                    return View(model);
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserTypeListMessage>>>(body);
                model.UserTypes = apiResponse?.Data ?? new List<UserTypeListMessage>();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Unexpected error: {ex.Message}";
            }

            return View(model);
        }

        // GET: /UserType/CreateUserType → show creation form
        [HttpGet]
        public IActionResult CreateUserType() => View(new CreateUserTypeMessage());

        // POST: /UserType/CreateUserType → call API to create
        [HttpPost]
        public async Task<IActionResult> CreateUserType(CreateUserTypeMessage dto)
        {
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError("", "Access token is missing.");
                    return View(dto); // Return the same view with error
                }

                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var client = _httpClientFactory.CreateClient("Library.UserApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(
                    $"api/usertype?createdByUserId={currentUserId}",
                    JsonContent.Create(dto)
                );

                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", $"Failed to create user type: {body}");
                    return View(dto); // Return the same view with error
                }

                // Success - use TempData to show a one-time success message
                TempData["SuccessMessage"] = "User type added successfully!";
                return RedirectToAction("Index", "UserType");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected exception: {ex.Message}");
                return View(dto); // Return the same view with error
            }
        }

        // GET: /UserType/UpdateUserType/{id} → show edit form
        [HttpGet]
        public async Task<IActionResult> UpdateUserType(int id)
        {
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                    return View("Error");

                var client = _httpClientFactory.CreateClient("Library.UserApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"api/usertype/query/{id}");
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = $"Failed to load user type: {body}";
                    return RedirectToAction("Index");
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserTypeListMessage>>(body);
                var dto = new UpdateUserTypeMessage
                {
                    Id = id,
                    Role = apiResponse?.Data?.Role ?? ""
                };

                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Error");
            }
        }

        // POST: /UserType/UpdateUserType → update API
        [HttpPost]
        public async Task<IActionResult> UpdateUserType(UpdateUserTypeMessage dto)
        {
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "Access token missing." });

                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var client = _httpClientFactory.CreateClient("Library.UserApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsync($"api/usertype/{dto.Id}?userId={currentUserId}", JsonContent.Create(dto));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Failed to update user type: {error}" });
                }

                return Json(new { success = true, message = "User type updated successfully!", role = dto.Role, id = dto.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /UserType/ArchiveUserType → AJAX
        [HttpPost]
        public async Task<IActionResult> ArchiveUserType([FromBody] int id)
        {
            try
            {
                var token = GetAccessToken();
                if (string.IsNullOrEmpty(token))
                    return Json(new { success = false, message = "Access token missing." });

                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var client = _httpClientFactory.CreateClient("Library.UserApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsync($"api/usertype/archive/{id}?userId={currentUserId}", null);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Failed to archive user type: {error}" });
                }

                return Json(new { success = true, message = "User type archived successfully!", id = id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}