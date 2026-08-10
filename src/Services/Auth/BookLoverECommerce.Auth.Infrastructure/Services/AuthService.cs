using BookLoverECommerce.Auth.Application.DTOs;
using BookLoverECommerce.Auth.Application.Exceptions;
using BookLoverECommerce.Auth.Application.Interfaces;
using BookLoverECommerce.Auth.Domain.Constants;
using BookLoverECommerce.Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BookLoverECommerce.Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUsername =
            await _userManager.FindByNameAsync(username);

        if (existingUsername is not null)
        {
            throw new AuthException(
                "The username is already registered.");
        }

        var existingEmail =
            await _userManager.FindByEmailAsync(email);

        if (existingEmail is not null)
        {
            throw new AuthException(
                "The email address is already registered.");
        }

        var user = new ApplicationUser
        {
            FullName = request.FullName.Trim(),
            UserName = username,
            Email = email,
            EmailConfirmed = true
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                createResult.Errors.Select(
                    error => error.Description));

            throw new AuthException(
                $"Registration failed: {errors}");
        }

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                UserRoles.Customer);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(
                    error => error.Description));

            throw new AuthException(
                $"Customer role assignment failed: {errors}");
        }

        return await _tokenService.CreateTokenAsync(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request)
    {
        var usernameOrEmail =
            request.UsernameOrEmail.Trim();

        ApplicationUser? user;

        if (usernameOrEmail.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(
                usernameOrEmail.ToLowerInvariant());
        }
        else
        {
            user = await _userManager.FindByNameAsync(
                usernameOrEmail);
        }

        if (user is null)
        {
            return null;
        }

        var passwordIsValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordIsValid)
        {
            return null;
        }

        return await _tokenService.CreateTokenAsync(user);
    }
    public async Task<ProfileResponse?> GetProfileAsync(
    string userId)
{
    var user =
        await _userManager.FindByIdAsync(userId);

    if (user is null)
    {
        return null;
    }

    var roles =
        await _userManager.GetRolesAsync(user);

    return new ProfileResponse
    {
        UserId = user.Id,
        Username = user.UserName ?? string.Empty,
        Email = user.Email ?? string.Empty,
        FullName = user.FullName,
        Roles = roles
    };
}
public async Task<ProfileResponse?> UpdateProfileAsync(
    string userId,
    UpdateProfileRequest request)
{
    var user =
        await _userManager.FindByIdAsync(userId);

    if (user is null)
    {
        return null;
    }

    var username =
        request.Username.Trim();

    var existingUsername =
        await _userManager.FindByNameAsync(
            username);

    if (existingUsername is not null &&
        existingUsername.Id != user.Id)
    {
        throw new AuthException(
            "The username is already being used.");
    }

    user.FullName =
        request.FullName.Trim();

    var usernameResult =
        await _userManager.SetUserNameAsync(
            user,
            username);

    if (!usernameResult.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                usernameResult.Errors.Select(
                    error => error.Description));

        throw new AuthException(
            $"Profile update failed: {errors}");
    }

    var updateResult =
        await _userManager.UpdateAsync(user);

    if (!updateResult.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                updateResult.Errors.Select(
                    error => error.Description));

        throw new AuthException(
            $"Profile update failed: {errors}");
    }

    return await GetProfileAsync(user.Id);
}
public async Task ChangeEmailAsync(
    string userId,
    ChangeEmailRequest request)
{
    var user =
        await _userManager.FindByIdAsync(
            userId);

    if (user is null)
    {
        throw new AuthException(
            "User account was not found.");
    }

    var newEmail =
        request.NewEmail
            .Trim()
            .ToLowerInvariant();

    // Check whether another account
    // already uses this email.
    var existingUser =
        await _userManager.FindByEmailAsync(
            newEmail);

    if (existingUser is not null &&
        existingUser.Id != user.Id)
    {
        throw new AuthException(
            "The email address is already registered.");
    }

    // If user entered the same email,
    // there is nothing to change.
    if (string.Equals(
            user.Email,
            newEmail,
            StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    // Change email directly.
    var result =
        await _userManager.SetEmailAsync(
            user,
            newEmail);

    if (!result.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                result.Errors.Select(
                    error =>
                        error.Description));

        throw new AuthException(
            $"Email update failed: {errors}");
    }

    // Your project currently treats
    // registered emails as confirmed.
    user.EmailConfirmed = true;

    var updateResult =
        await _userManager.UpdateAsync(
            user);

    if (!updateResult.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                updateResult.Errors.Select(
                    error =>
                        error.Description));

        throw new AuthException(
            $"Email update failed: {errors}");
    }
}
public async Task ChangePasswordAsync(
    string userId,
    ChangePasswordRequest request)
{
    var user =
        await _userManager.FindByIdAsync(userId);

    if (user is null)
    {
        throw new AuthException(
            "User account was not found.");
    }

    var result =
        await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

    if (!result.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                result.Errors.Select(
                    error => error.Description));

        throw new AuthException(
            $"Password update failed: {errors}");
    }
}
public async Task<ForgotPasswordResponse> ForgotPasswordAsync(
    ForgotPasswordRequest request)
{
    var email =
        request.Email
            .Trim()
            .ToLowerInvariant();

    var user =
        await _userManager.FindByEmailAsync(
            email);

    // Important:
    // Do not reveal whether an account exists.
    if (user is null)
    {
        return new ForgotPasswordResponse
        {
            Message =
                "If an account exists for this email, " +
                "password reset instructions have been generated."
        };
    }

    var resetToken =
        await _userManager
            .GeneratePasswordResetTokenAsync(
                user);

    return new ForgotPasswordResponse
    {
        Message =
            "If an account exists for this email, " +
            "password reset instructions have been generated.",

        ResetToken =
            resetToken
    };
}
public async Task ResetPasswordAsync(
    ResetPasswordRequest request)
{
    var email =
        request.Email
            .Trim()
            .ToLowerInvariant();

    var user =
        await _userManager.FindByEmailAsync(
            email);

    if (user is null)
    {
        throw new AuthException(
            "The password reset request is invalid.");
    }

    var result =
        await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);

    if (!result.Succeeded)
    {
        var errors =
            string.Join(
                "; ",
                result.Errors.Select(
                    error =>
                        error.Description));

        throw new AuthException(
            $"Password reset failed: {errors}");
    }
}
}