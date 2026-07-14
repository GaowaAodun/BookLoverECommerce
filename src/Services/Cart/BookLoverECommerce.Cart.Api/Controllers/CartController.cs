using System.Security.Claims;
using BookLoverECommerce.Cart.Application.DTOs;
using BookLoverECommerce.Cart.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Cart.Api.Controllers;

[ApiController]
[Authorize]
[Route("cart/{userId}")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartResponse>> GetCart(
        string userId,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCart(userId))
        {
            return Forbid();
        }

        var cart = await _cartService.GetCartAsync(
            userId,
            cancellationToken);

        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartResponse>> AddItem(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCart(userId))
        {
            return Forbid();
        }

        var cart = await _cartService.AddItemAsync(
            userId,
            request,
            cancellationToken);

        return Ok(cart);
    }

    [HttpDelete("items")]
    public async Task<ActionResult<CartResponse>> RemoveItem(
        string userId,
        RemoveCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessCart(userId))
        {
            return Forbid();
        }

        var cart = await _cartService.RemoveItemAsync(
            userId,
            request,
            cancellationToken);

        if (cart is null)
        {
            return NotFound(new
            {
                message = "Cart or product was not found."
            });
        }

        return Ok(cart);
    }

    private bool CanAccessCart(string userId)
    {
        var authenticatedUserId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        return authenticatedUserId == userId ||
               User.IsInRole("Admin");
    }
}