using Microsoft.AspNetCore.Identity;

namespace BookLoverECommerce.Auth.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}