using BookLoverECommerce.Products.Application.Categories;
using BookLoverECommerce.Products.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Products.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(
        ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryDto>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>>
        GetCategories(
            CancellationToken cancellationToken)
    {
        var categories =
            await _categoryService.GetActiveAsync(
                cancellationToken);

        return Ok(categories);
    }
}