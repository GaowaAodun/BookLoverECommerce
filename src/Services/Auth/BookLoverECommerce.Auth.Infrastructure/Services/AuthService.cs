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
}