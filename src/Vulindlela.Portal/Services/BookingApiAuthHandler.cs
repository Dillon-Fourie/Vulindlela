
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Vulindlela.Portal.Services;

public class BookingApiAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtTokenService _jwtTokenService;

    public BookingApiAuthHandler(
        IHttpContextAccessor httpContextAccessor,
        JwtTokenService jwtTokenService)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtTokenService = jwtTokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "You must be signed in to access the Booking API.");
        }

        var token = _jwtTokenService.GenerateToken(user);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
