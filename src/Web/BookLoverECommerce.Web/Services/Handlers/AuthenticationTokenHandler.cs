using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace BookLoverECommerce.Web.Services.Handlers;

public sealed class AuthenticationTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationTokenHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var token =
                await httpContext.GetTokenAsync(
                    "access_token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}