using System.Security.Claims;
using BookLoverECommerce.Auth.Application.DTOs;
using BookLoverECommerce.Auth.Application.Exceptions;
using BookLoverECommerce.Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLoverECommerce.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }


    // =====================================
    // REGISTER
    // =====================================

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
                await _authService.RegisterAsync(
                    request);

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


    // =====================================
    // LOGIN
    // =====================================

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
            await _authService.LoginAsync(
                request);

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


    // =====================================
    // GET CURRENT PROFILE
    // =====================================

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<ProfileResponse>>
        GetProfile()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var profile =
            await _authService.GetProfileAsync(
                userId);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "User profile was not found."
            });
        }

        return Ok(profile);
    }


    // =====================================
    // UPDATE PROFILE
    // =====================================

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<ProfileResponse>>
        UpdateProfile(
            UpdateProfileRequest request)
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            var profile =
                await _authService.UpdateProfileAsync(
                    userId,
                    request);

            if (profile is null)
            {
                return NotFound(new
                {
                    message =
                        "User profile was not found."
                });
            }

            return Ok(profile);
        }
        catch (AuthException exception)
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
    }


    // =====================================
    // CHANGE EMAIL
    // =====================================

    [Authorize]
    [HttpPut("profile/email")]
    public async Task<IActionResult> ChangeEmail(
        ChangeEmailRequest request)
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _authService.ChangeEmailAsync(
                userId,
                request);

            return Ok(new
            {
                message =
                    "Email updated successfully."
            });
        }
        catch (AuthException exception)
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
    }


    // =====================================
    // CHANGE PASSWORD
    // =====================================

    [Authorize]
    [HttpPut("profile/password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request)
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _authService.ChangePasswordAsync(
                userId,
                request);

            return Ok(new
            {
                message =
                    "Password updated successfully."
            });
        }
        catch (AuthException exception)
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
    }
    [HttpPost("forgot-password")]
public async Task<ActionResult<ForgotPasswordResponse>>
    ForgotPassword(
        ForgotPasswordRequest request)
{
    var response =
        await _authService.ForgotPasswordAsync(
            request);

    return Ok(response);
}
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword(
    ResetPasswordRequest request)
{
    try
    {
        await _authService.ResetPasswordAsync(
            request);

        return Ok(new
        {
            message =
                "Your password has been reset successfully."
        });
    }
    catch (AuthException exception)
    {
        return BadRequest(new
        {
            message = exception.Message
        });
    }
}
}