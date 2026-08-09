using BookLoverECommerce.Products.Application.Categories;
using BookLoverECommerce.Products.Application.DTOs;
using BookLoverECommerce.Products.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Products.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Public: used by product forms and storefront clients.
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(IReadOnlyList<CategoryDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>>
        GetActiveCategories(
            CancellationToken cancellationToken)
    {
        var categories =
            await _categoryService.GetActiveAsync(
                cancellationToken);

        return Ok(categories);
    }

    // Admin: includes inactive categories.
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(IReadOnlyList<CategoryDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>>
        GetAllCategoriesForAdmin(
            CancellationToken cancellationToken)
    {
        var categories =
            await _categoryService.GetAllForAdminAsync(
                cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(CategoryDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> GetCategory(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var category =
                await _categoryService.GetByIdAsync(
                    id,
                    cancellationToken);

            return Ok(category);
        }
        catch (CategoryNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(CategoryDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var category =
                await _categoryService.CreateAsync(
                    command,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category);
        }
        catch (DuplicateCategoryNameException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
        catch (CategoryNotFoundException exception)
        {
            return BadRequest(new
            {
                message =
                    $"The selected parent category is invalid. " +
                    exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (InvalidCategoryHierarchyException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(CategoryDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var category =
                await _categoryService.UpdateAsync(
                    id,
                    command,
                    cancellationToken);

            return Ok(category);
        }
        catch (DuplicateCategoryNameException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
        catch (CategoryNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
        catch (InvalidCategoryHierarchyException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateCategory(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _categoryService.ActivateAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (CategoryNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (InvalidCategoryHierarchyException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPatch("{id:int}/deactivate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateCategory(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _categoryService.DeactivateAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (CategoryNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (InvalidCategoryHierarchyException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _categoryService.DeleteAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (CategoryNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (CategoryInUseException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }
}