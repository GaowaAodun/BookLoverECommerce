using System.Security.Claims;
using BookLoverECommerce.Products.Api.Contracts.Products;
using BookLoverECommerce.Products.Application.DTOs;
using BookLoverECommerce.Products.Application.Exceptions;
using BookLoverECommerce.Products.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BookLoverECommerce.Contracts.Products;
using MassTransit;

namespace BookLoverECommerce.Products.Api.Controllers;

[ApiController]
[Route("/api/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IPublishEndpoint _publishEndpoint;

    
    public ProductsController(
        IProductService productService,
        IPublishEndpoint publishEndpoint)
    {
        _productService = productService;
        _publishEndpoint = publishEndpoint;
    }

    // GET /products
    // GET /products?productIds=1,2,3
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        [FromQuery] string? productIds,
        CancellationToken cancellationToken)
    {
        var parsedIds = ParseProductIds(productIds);

        if (parsedIds is null)
        {
            return BadRequest(new
            {
                message =
                    "productIds must contain comma-separated positive integers."
            });
        }

        var products = await _productService.GetProductsAsync(
            parsedIds,
            cancellationToken);

        return Ok(products);
    }

    // POST /products
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new
            {
                message = "The token does not contain a valid user ID."
            });
        }

        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Sku,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            request.ProductType,
            userId,
            request.Brand,
            request.ThumbnailUrl);

        try
        {
            var product = await _productService.CreateAsync(
                command,
                cancellationToken);

            // publish from the Products controller immediately after product creation
            await _publishEndpoint.Publish(
                new ProductCreated(
                    product.Id,
                    product.Name,
                    product.Sku,
                    product.Price,
                    DateTimeOffset.UtcNow),
                    cancellationToken);

            return Created(
                $"/products/{product.Id}",
                product);
        }
        catch (DuplicateSkuException exception)
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
    }

    // DELETE /products/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProduct(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _productService.DeleteAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (ProductNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

    // PATCH /products/{id}/archive
    [HttpPatch("{id:int}/archive")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveProduct(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _productService.ArchiveAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (ProductNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
    }

    private static IReadOnlyCollection<int>? ParseProductIds(
        string? productIds)
    {
        if (string.IsNullOrWhiteSpace(productIds))
        {
            return Array.Empty<int>();
        }

        var values = productIds.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        var parsedIds = new List<int>();

        foreach (var value in values)
        {
            if (!int.TryParse(value, out var id) || id <= 0)
            {
                return null;
            }

            parsedIds.Add(id);
        }

        return parsedIds
            .Distinct()
            .ToArray();
    }

    [HttpPatch("{id:int}/publish")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PublishProduct(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _productService.PublishAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ProductNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

}