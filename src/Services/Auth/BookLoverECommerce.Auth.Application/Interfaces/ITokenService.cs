using BookLoverECommerce.Auth.Application.DTOs;
using BookLoverECommerce.Auth.Domain.Entities;

namespace BookLoverECommerce.Auth.Application.Interfaces;

public interface ITokenService
{
    Task<AuthResponse> CreateTokenAsync(ApplicationUser user);
}