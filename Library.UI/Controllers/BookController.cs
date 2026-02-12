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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Library.UI.Controllers
{
    [Authorize(Policy = PermissionNames.BookBasic)]
    public class BookController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ApiSettings _apiSettings;
        private readonly ILibraryDataHelper _libraryDataHelper;

        public BookController(
            IApiClient apiClient,
            IOptions<ApiSettings> apiSettings,
            ILibraryDataHelper libraryDataHelper)
        {
            _apiClient = apiClient;
            _apiSettings = apiSettings.Value;
            _libraryDataHelper = libraryDataHelper;
        }

        //GET: /Book 
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1, int pageSize = 10,
            string search = "", string filter = "",
            bool ajax = false)
        {
            var model = new BookViewModel();

            try
            {
                pageSize = pageSize > 0 ? pageSize : 10;

                var baseQueryUrl = ApiUrlBuilder.ForQuery(_apiSettings.LibraryApi.Endpoints.Book);

                var queryParams = new Dictionary<string, string?>
                {
                    ["page"] = page.ToString(),
                    ["pageSize"] = pageSize.ToString(),
                    ["search"] = search,
                    ["filter"] = filter
                };

                //appends search params
                var finalUrl = QueryHelpers.AddQueryString($"{baseQueryUrl}/search", queryParams);

                var response = await _apiClient.GetAsync<ApiResponse<PagedResultDto<BookListDto>>>(finalUrl, apiName: "LibraryApi");

                model.Books = response?.Data?.Items ?? new List<BookListDto>();
                model.TotalCount = response?.Data?.TotalCount ?? 0;
                model.Page = page;
                model.PageSize = pageSize;
                model.Search = search;
                model.Filter = filter;

                //Load categories (for filtering or display)
                model.Categories = await _libraryDataHelper.GetCategoriesAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Failed to load books: {ex.Message}";
            }

            if (ajax)
                return PartialView("_BooksTablePartial", model);

            return View(model);
        }

        //GET: /Book/CreateBook
        [HttpGet]
        [Authorize(Policy = PermissionNames.BookManage)]
        public async Task<IActionResult> CreateBook()
        {
            var model = new CreateBookDto();

            //Load categories, publishers, and Authors as SelectLists
            ViewBag.Categories = await _libraryDataHelper.GetCategoriesSelectListAsync();
            ViewBag.Publishers = await _libraryDataHelper.GetPublishersSelectListAsync();
            ViewBag.Authors = await _libraryDataHelper.GetAuthorsSelectListAsync();

            return View(model);
        }

        //POST: /Book/CreateBook
        [HttpPost]
        [Authorize(Policy = PermissionNames.BookManage)]
        public async Task<IActionResult> CreateBook(CreateBookDto dto)
        {
            if (!ModelState.IsValid)
            {
                //Reload dropdowns if validation fails
                ViewBag.Categories = await _libraryDataHelper.GetCategoriesSelectListAsync(dto.CategoryId);
                ViewBag.Publishers = await _libraryDataHelper.GetPublishersSelectListAsync(dto.PublisherId);
                ViewBag.Authors = await _libraryDataHelper.GetAuthorsSelectListAsync(dto.AuthorId);
                return View(dto);
            }

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

                    //Reload dropdowns
                    ViewBag.Categories = await _libraryDataHelper.GetCategoriesSelectListAsync(dto.CategoryId);
                    ViewBag.Publishers = await _libraryDataHelper.GetPublishersSelectListAsync(dto.PublisherId);
                    ViewBag.Authors = await _libraryDataHelper.GetAuthorsSelectListAsync(dto.AuthorId);

                    return View(dto);
                }

                TempData["SuccessMessage"] = "Book added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");

                // Reload dropdowns
                ViewBag.Categories = await _libraryDataHelper.GetCategoriesSelectListAsync(dto.CategoryId);
                ViewBag.Publishers = await _libraryDataHelper.GetPublishersSelectListAsync(dto.PublisherId);
                ViewBag.Authors = await _libraryDataHelper.GetAuthorsSelectListAsync(dto.AuthorId);

                return View(dto);
            }
        }

        //POST: /Book/UpdateBook (AJAX)
        [HttpPost]
        [Authorize(Policy = PermissionNames.BookManage)]
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

        //POST: /Book/ArchiveBook (AJAX)
        [HttpPost]
        [Authorize(Policy = PermissionNames.BookManage)]
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