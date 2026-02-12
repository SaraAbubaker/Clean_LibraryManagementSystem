using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos.Author;
using Library.Common.DTOs.LibraryDtos.Category;
using Library.Common.DTOs.LibraryDtos.Publisher;
using Library.UI.Models.String_constant;
using Library.UI.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Collections;

namespace Library.UI.Helpers
{
    public class LibraryDataHelper : ILibraryDataHelper
    {
        private readonly ApiSettings _apiSettings;
        private readonly IApiClient _apiClient;

        public LibraryDataHelper(IOptions<ApiSettings> apiSettings, IApiClient apiClient)
        {
            _apiSettings = apiSettings.Value;
            _apiClient = apiClient;
        }

        public async Task<List<CategoryListDto>> GetCategoriesAsync()
        {
            try
            {
                var response = await _apiClient.GetQueryAsync<ApiResponse<List<CategoryListDto>>>(
                    _apiSettings.LibraryApi.Endpoints.Category);
                return response?.Data ?? new List<CategoryListDto>();
            }
            catch
            {
                return new List<CategoryListDto>();
            }
        }

        public async Task<List<PublisherListDto>> GetPublishersAsync()
        {
            try
            {
                var response = await _apiClient.GetQueryAsync<ApiResponse<List<PublisherListDto>>>(
                    _apiSettings.LibraryApi.Endpoints.Publisher);
                return response?.Data ?? new List<PublisherListDto>();
            }
            catch
            {
                return new List<PublisherListDto>();
            }
        }

        public async Task<List<AuthorListDto>> GetAuthorsAsync()
        {
            try
            {
                var response = await _apiClient.GetQueryAsync<ApiResponse<List<AuthorListDto>>>(
                    _apiSettings.LibraryApi.Endpoints.Author);
                return response?.Data ?? new List<AuthorListDto>();
            }
            catch
            {
                return new List<AuthorListDto>();
            }
        }

        public async Task<SelectList> GetCategoriesSelectListAsync(int? selectedId = null)
        {
            var categories = await GetCategoriesAsync();
            return new SelectList(categories, "Id", "Name", selectedId);
        }

        public async Task<SelectList> GetPublishersSelectListAsync(int? selectedId = null)
        {
            var publishers = await GetPublishersAsync();
            return new SelectList(publishers, "Id", "Name", selectedId);
        }

        public async Task<SelectList> GetAuthorsSelectListAsync(int? selectedId = null)
        {
            var publishers = await GetAuthorsAsync();
            return new SelectList(publishers, "Id", "Name", selectedId);
        }
    }
}