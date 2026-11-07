
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Tests;

public class BookingSecurityTests
{
    [Fact]
    public async Task UnauthenticatedRequest_ReturnsUnauthorized()
    {
        using var factory = new SecurityTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/appointments/citizen/TEST-CITIZEN-005");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Citizen_CannotViewAnotherCitizensAppointments()
    {
        using var factory = new SecurityTestFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Test",
                "Citizen:TEST-CITIZEN-005");

        var response = await client.GetAsync(
            "/api/appointments/citizen/TEST-CITIZEN-006");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Official_CanViewCitizenAppointments()
    {
        using var factory = new SecurityTestFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Test", "Official:official1");

        var response = await client.GetAsync(
            "/api/appointments/citizen/TEST-CITIZEN-005");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SuccessfulRead_CreatesAuditLog()
    {
        using var factory = new SecurityTestFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Test",
                "Citizen:TEST-CITIZEN-005");

        var response = await client.GetAsync(
            "/api/appointments/citizen/TEST-CITIZEN-005");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<BookingContext>();

        var log = await db.BookingAccessLogs
            .SingleAsync();

        Assert.Equal("TEST-CITIZEN-005", log.CitizenId);
        Assert.Equal("Citizen", log.AccessedByRole);
        Assert.Equal("Viewed citizen appointments", log.Reason);
    }
}

public class SecurityTestFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    public SecurityTestFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting(
            "Jwt:Key",
            Convert.ToBase64String(new byte[64]));

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<BookingContext>(options =>
                options.UseSqlite(_connection));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
                options.DefaultForbidScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                "Test",
                _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();

        if (!header.StartsWith("Test ", StringComparison.Ordinal))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var value = header["Test ".Length..];

        string username;
        string role;
        string citizenId;

        if (value.StartsWith("Citizen:", StringComparison.Ordinal))
        {
            role = "Citizen";
            citizenId = value["Citizen:".Length..];
            username = "testcitizen";
        }
        else if (value.StartsWith("Official:", StringComparison.Ordinal))
        {
            role = "Official";
            citizenId = string.Empty;
            username = value["Official:".Length..];
        }
        else
        {
            return Task.FromResult(
                AuthenticateResult.Fail("Invalid test identity."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("CitizenId", citizenId)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
