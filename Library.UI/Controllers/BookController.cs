using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos;
using Library.Common.DTOs.LibraryDtos.Book;
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
    [Authorize(Policy = PermissionNames.BookManage)]
    public class BookController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ApiSettings _apiSettings;

        public BookController(
            IApiClient apiClient,
            IOptions<ApiSettings> apiSettings)
        {
            _apiClient = apiClient;
            _apiSettings = apiSettings.Value;
        }

        // GET: /Book
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new BookViewModel();

            try
            {
                var response = await _apiClient.GetAsync<ApiPagedResponse<PagedResultDto<BookListDto>>>(
                    _apiSettings.LibraryApi.Endpoints.Book + "/query/search?page=1",
                    apiName: "LibraryApi"
                );

                model.Books = response?.Data?.Items ?? new List<BookListDto>();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Failed to load books: {ex.Message}";
            }

            return View(model);
        }

        // GET: /Book/CreateBook
        [HttpGet]
        public IActionResult CreateBook() => View(new CreateBookDto());

        // POST: /Book/CreateBook
        [HttpPost]
        public async Task<IActionResult> CreateBook(CreateBookDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PostAsync(
                    _apiSettings.LibraryApi.Endpoints.Book,
                    dto,
                    currentUserId,
                    apiName: "LibraryApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", error);
                    return View(dto);
                }

                TempData["SuccessMessage"] = "Book added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return View(dto);
            }
        }

        // POST: /Book/UpdateBook (AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateBook([FromBody] UpdateBookDto dto)
        {
            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PutAsync(
                    _apiSettings.LibraryApi.Endpoints.Book,
                    dto.Id,
                    dto,
                    currentUserId,
                    apiName: "LibraryApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }

                return Json(new { success = true, message = "Book updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Book/ArchiveBook (AJAX)
        [HttpPost]
        public async Task<IActionResult> ArchiveBook([FromBody] int id)
        {
            try
            {
                int currentUserId = GetUserHelper.GetCurrentUserId(User);

                var response = await _apiClient.PutArchiveAsync(
                    _apiSettings.LibraryApi.Endpoints.Book,
                    id,
                    currentUserId,
                    apiName: "LibraryApi"
                );

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = error });
                }

                return Json(new { success = true, message = "Book archived successfully!", id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}