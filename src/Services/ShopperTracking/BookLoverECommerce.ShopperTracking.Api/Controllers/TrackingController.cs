using BookLoverECommerce.ShopperTracking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLoverECommerce.ShopperTracking.Api.Controllers;

[ApiController]
[Route("api/tracking")]
[Authorize(Roles = "Admin")]
public sealed class TrackingController : ControllerBase
{
    private readonly ShopperTrackingDbContext _dbContext;

    public TrackingController(
        ShopperTrackingDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    // =========================================================
    // GET ALL SHOPPER EVENTS
    // ADMIN ONLY
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var events =
            await _dbContext.ShopperEvents
                .AsNoTracking()
                .OrderByDescending(
                    x => x.OccurredAt)
                .ToListAsync(
                    cancellationToken);

        return Ok(events);
    }


    // =========================================================
    // GET EVENTS BY CUSTOMER
    // =========================================================

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(
        string customerId,
        CancellationToken cancellationToken)
    {
        var events =
            await _dbContext.ShopperEvents
                .AsNoTracking()
                .Where(
                    x =>
                        x.CustomerId ==
                        customerId)
                .OrderByDescending(
                    x => x.OccurredAt)
                .ToListAsync(
                    cancellationToken);

        return Ok(events);
    }


    // =========================================================
    // GET EVENTS BY PRODUCT
    // =========================================================

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var events =
            await _dbContext.ShopperEvents
                .AsNoTracking()
                .Where(
                    x =>
                        x.ProductId ==
                        productId)
                .OrderByDescending(
                    x => x.OccurredAt)
                .ToListAsync(
                    cancellationToken);

        return Ok(events);
    }


    // =========================================================
    // GET EVENTS BY TYPE
    // Example:
    // /api/tracking/type/ItemAddedToCart
    // =========================================================

    [HttpGet("type/{eventType}")]
    public async Task<IActionResult> GetByType(
        string eventType,
        CancellationToken cancellationToken)
    {
        var events =
            await _dbContext.ShopperEvents
                .AsNoTracking()
                .Where(
                    x =>
                        x.EventType ==
                        eventType)
                .OrderByDescending(
                    x => x.OccurredAt)
                .ToListAsync(
                    cancellationToken);

        return Ok(events);
    }
}