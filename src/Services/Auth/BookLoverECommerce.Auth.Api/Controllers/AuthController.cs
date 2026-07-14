using BookLoverECommerce.Auth.Application.DTOs;
using BookLoverECommerce.Auth.Application.Exceptions;
using BookLoverECommerce.Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Auth.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(
        typeof(AuthResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        try
        {
            var response =
                await _authService.RegisterAsync(request);

            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }
        catch (AuthException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AuthResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var response =
            await _authService.LoginAsync(request);

        if (response is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid username/email or password."
            });
        }

        return Ok(response);
    }
}