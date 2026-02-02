using Library.Common.DTOs.ApiResponseDtos;
using Library.Common.DTOs.LibraryDtos.Category;
using Library.Common.Exceptions;
using Library.Common.StringConstants;
using Library.Services.Interfaces;
using Library.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = PermissionNames.CategoryBasic)]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Policy = PermissionNames.CategoryManage)]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var created = await _service.CreateCategoryAsync(dto, userId);

                return Ok(ApiResponseHelper.Success(created));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CreateCategoryDto>(ex.Message));
            }
        }

        [HttpGet("query")]
        public IActionResult GetAllCategoriesQuery()
        {
            try
            {
                var query = _service.GetAllCategoriesQuery().ToList();
                return Ok(ApiResponseHelper.Success(query));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<List<CategoryListDto>>(ex.Message));
            }
        }

        [HttpGet("query/{id}")]
        public IActionResult GetCategoryByIdQuery(int id)
        {
            try
            {
                var category = _service.GetCategoryByIdQuery(id).FirstOrDefault();

                if (category == null)
                    return NotFound(ApiResponseHelper.Failure<CategoryListDto>($"Category with id {id} was not found."));

                return Ok(ApiResponseHelper.Success(category));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CategoryListDto>(ex.Message));
            }
        }

        [HttpPut]
        [Authorize(Policy = PermissionNames.CategoryManage)]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto dto)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var updated = await _service.UpdateCategoryAsync(dto, userId);

                if (updated == null)
                    return NotFound(ApiResponseHelper.Failure<UpdateCategoryDto>("Category not found."));

                return Ok(ApiResponseHelper.Success(updated));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateCategoryDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        [Authorize(Policy = PermissionNames.CategoryManage)]
        public async Task<IActionResult> ArchiveCategory(int id)
        {
            try
            {
                int userId = UserClaimHelper.GetUserClaim(User);
                var success = await _service.ArchiveCategoryAsync(id, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<CategoryListDto>("Category not found."));

                return Ok(ApiResponseHelper.Success(new { Message = "Category archived successfully." }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (ConflictException ex)
            {
                return Conflict(ApiResponseHelper.Failure<CategoryListDto>(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<CategoryListDto>(ex.Message));
            }
        }
    }
}