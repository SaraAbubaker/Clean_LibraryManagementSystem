using Library.Services.Interfaces;
using Library.Shared.DTOs.ApiResponses;
using Library.Shared.DTOs.Category;
using Library.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory( CreateCategoryDto dto, [FromQuery] int userId)
        {
            try
            {
                var created = await _service.CreateCategoryAsync(dto, userId);
                return Ok(ApiResponseHelper.Success(created));
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
        public async Task<IActionResult> UpdateCategory(UpdateCategoryDto dto, [FromQuery] int userId)
        {
            try
            {
                var updated = await _service.UpdateCategoryAsync(dto, userId);

                if (updated == null)
                    return NotFound(ApiResponseHelper.Failure<UpdateCategoryDto>("Category not found."));

                return Ok(ApiResponseHelper.Success(updated));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseHelper.Failure<UpdateCategoryDto>(ex.Message));
            }
        }

        [HttpPut("archive/{id}")]
        public async Task<IActionResult> ArchiveCategory(int id, [FromQuery] int userId)
        {
            try
            {
                var success = await _service.ArchiveCategoryAsync(id, userId);

                if (!success)
                    return NotFound(ApiResponseHelper.Failure<CategoryListDto>("Category not found."));

                return Ok(ApiResponseHelper.Success(new { Message = "Category archived successfully." }));
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
