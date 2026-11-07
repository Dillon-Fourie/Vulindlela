
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Vulindlela.Portal.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException(
                "A signed-in user is required.");
        }

        var citizenId = user.FindFirstValue("CitizenId") ?? "";
        var username = user.Identity.Name
            ?? throw new UnauthorizedAccessException(
                "The signed-in user has no username.");

        var role = user.IsInRole("Official")
            ? "Official"
            : user.IsInRole("Citizen")
                ? "Citizen"
                : throw new UnauthorizedAccessException(
                    "The signed-in user has no recognised role.");

        if (role == "Citizen" && string.IsNullOrWhiteSpace(citizenId))
        {
            throw new UnauthorizedAccessException(
                "The citizen account has no citizen ID.");
        }

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT signing key is missing.");

        var signingKey = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtKey));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub,
                user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? username),

            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim("role", role),
            new Claim("CitizenId", citizenId)
        };

        var token = new JwtSecurityToken(
            issuer: "Vulindlela.Portal",
            audience: "Vulindlela.Booking.Api",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
