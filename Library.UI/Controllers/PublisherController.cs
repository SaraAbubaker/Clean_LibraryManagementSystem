using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos.Publisher;
using Library.Common.StringConstants;
using Library.UI.Helpers;
using Library.UI.Models.String_constant;
using Library.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Library.UI.Controllers
{
    [Authorize(Policy = PermissionNames.PublisherManage)]
    public class PublisherController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ApiSettings _apiSettings;

        public PublisherController(
            IApiClient apiClient,
            IOptions<ApiSettings> apiSettings)
        {
            _apiClient = apiClient;
            _apiSettings = apiSettings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data." });

            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PostAsync(
                    _apiSettings.LibraryApi.Endpoints.Publisher,
                    dto,
                    currentUserId,
                    apiName: "LibraryApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }

                var apiResult = await response.Content
                    .ReadFromJsonAsync<ApiResponse<PublisherListDto>>();

                return Json(new
                {
                    success = true,
                    id = apiResult?.Data?.Id,
                    name = apiResult?.Data?.Name
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
