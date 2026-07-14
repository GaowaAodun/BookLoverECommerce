using BookLoverECommerce.Price.Api.Contracts.Prices;
using BookLoverECommerce.Price.Application.DTOs;
using BookLoverECommerce.Price.Application.Exceptions;
using BookLoverECommerce.Price.Application.Prices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Price.Api.Controllers;

[ApiController]
[Route("api/prices")]
public sealed class ProductPricesController : ControllerBase
{
    private readonly IProductPriceService _productPriceService;

    public ProductPricesController(
        IProductPriceService productPriceService)
    {
        _productPriceService = productPriceService;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ProductPriceDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ProductPriceDto>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        var prices = await _productPriceService.GetAllAsync(
            cancellationToken);

        return Ok(prices);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ProductPriceDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var price = await _productPriceService.GetByIdAsync(
                id,
                cancellationToken);

            return Ok(price);
        }
        catch (ProductPriceNotFoundException exception)
        {
            return NotFound(new
            {
                error = exception.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(
        typeof(ProductPriceDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceDto>> GetByProductId(
        Guid productId,
        CancellationToken cancellationToken)
    {
        try
        {
            var price =
                await _productPriceService.GetByProductIdAsync(
                    productId,
                    cancellationToken);

            return Ok(price);
        }
        catch (ProductPriceForProductNotFoundException exception)
        {
            return NotFound(new
            {
                error = exception.Message
            });
        }
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(
        typeof(ProductPriceDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductPriceDto>> Create(
        CreateProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateProductPriceCommand(
                request.ProductId,
                request.BasePrice,
                request.Currency,
                request.SalePrice,
                request.SaleStartDate,
                request.SaleEndDate);

            var createdPrice =
                await _productPriceService.CreateAsync(
                    command,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = createdPrice.Id
                },
                createdPrice);
        }
        catch (DuplicateProductPriceException exception)
        {
            return Conflict(new
            {
                error = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(
        typeof(ProductPriceDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceDto>> Update(
        Guid id,
        UpdateProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateProductPriceCommand(
                request.BasePrice,
                request.Currency,
                request.SalePrice,
                request.SaleStartDate,
                request.SaleEndDate);

            var updatedPrice =
                await _productPriceService.UpdateAsync(
                    id,
                    command,
                    cancellationToken);

            return Ok(updatedPrice);
        }
        catch (ProductPriceNotFoundException exception)
        {
            return NotFound(new
            {
                error = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                error = exception.Message
            });
        }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _productPriceService.DeleteAsync(
                id,
                cancellationToken);

            return NoContent();
        }
        catch (ProductPriceNotFoundException exception)
        {
            return NotFound(new
            {
                error = exception.Message
            });
        }
    }
}
