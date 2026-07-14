using BookLoverECommerce.Price.Domain.Enums;

namespace BookLoverECommerce.Price.Application.DTOs;

public sealed record CreateProductPriceCommand(
    Guid ProductId,
    decimal BasePrice,
    Currency Currency,
    decimal? SalePrice,
    DateTime? SaleStartDate,
    DateTime? SaleEndDate);
