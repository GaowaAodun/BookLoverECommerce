using System.Security.Claims;
using BookLoverECommerce.Order.Application.DTOs;
using BookLoverECommerce.Order.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Order.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        var order = await _orderService.CreateAsync(
            customerId,
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetMine),
            new { },
            order);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetMine(
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        var orders = await _orderService.GetMineAsync(
            customerId,
            cancellationToken);

        return Ok(orders);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllAsync(
            cancellationToken);

        return Ok(orders);
    }

    [HttpPost("{orderId:guid}/payment-succeeded")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkPaymentSucceeded(
    Guid orderId,
    PaymentSucceededRequest request,
    CancellationToken cancellationToken)
    {
        var updated = await _orderService.MarkPaymentSucceededAsync(
            orderId,
            request.PaymentReference,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{orderId:guid}/start-processing")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> StartProcessing(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var updated = await _orderService.StartProcessingAsync(
            orderId,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{orderId:guid}/ship")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Ship(
        Guid orderId,
        ShipOrderRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _orderService.ShipAsync(
            orderId,
            request,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{orderId:guid}/confirm-receipt")]
    public async Task<IActionResult> ConfirmReceipt(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        var updated = await _orderService.ConfirmReceiptAsync(
            orderId,
            customerId,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{orderId:guid}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Complete(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var updated = await _orderService.CompleteAsync(
            orderId,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();

        var updated = await _orderService.CancelByCustomerAsync(
            orderId,
            customerId,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    private string GetCustomerId()
    {
        var customerId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new UnauthorizedAccessException(
                "The access token does not contain a customer ID.");
        }

        return customerId;
    }
}