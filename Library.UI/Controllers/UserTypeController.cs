using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.UserApiDtos.UserTypeDtos;
using Library.Common.StringConstants;
using Library.UI.Helpers;
using Library.UI.Models;
using Library.UI.Models.String_constant;
using Library.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Library.UI.Controllers
{
    [Authorize(Policy = PermissionNames.UserManage)]
    public class UserTypeController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ApiSettings _apiSettings;

        public UserTypeController(
            IApiClient apiClient,
            IOptions<ApiSettings> apiSettings)
        {
            _apiClient = apiClient;
            _apiSettings = apiSettings.Value;
        }

        // GET: /UserType
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new UserTypeViewModel();

            try
            {
                var response = await _apiClient.GetQueryAsync<ApiResponse<List<UserTypeListMessage>>>(
                    _apiSettings.UserApi.Endpoints.UserType,
                    apiName: "UserApi"
                );

                model.UserTypes = response?.Data ?? new();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Failed to load user types: {ex.Message}";
            }

            return View(model);
        }

        // GET: /UserType/CreateUserType
        [HttpGet]
        public IActionResult CreateUserType() => View(new CreateUserTypeMessage());

        // POST: /UserType/CreateUserType
        [HttpPost]
        public async Task<IActionResult> CreateUserType(CreateUserTypeMessage dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PostAsync(
                    _apiSettings.UserApi.Endpoints.UserType,
                    dto,
                    currentUserId,
                    apiName: "UserApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                    return View(dto);
                }

                TempData["SuccessMessage"] = "User type added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return View(dto);
            }
        }

        // POST: /UserType/UpdateUserType (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateUserType([FromBody] UpdateUserTypeMessage dto)
        {
            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PutAsync(
                    _apiSettings.UserApi.Endpoints.UserType,
                    dto.Id,
                    dto,
                    currentUserId,
                    apiName: "UserApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }

                return Json(new { success = true, message = "User type updated successfully!", role = dto.Role, id = dto.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /UserType/ArchiveUserType (AJAX)
        [HttpPost]
        public async Task<IActionResult> ArchiveUserType([FromBody] int id)
        {
            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PutArchiveAsync(
                    _apiSettings.UserApi.Endpoints.UserType,
                    id,
                    currentUserId,
                    apiName: "UserApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }

                return Json(new { success = true, message = "User type archived successfully!", id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
