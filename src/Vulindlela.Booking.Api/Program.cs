
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Repositories;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Services;
using Vulindlela.Booking.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Enable OpenAPI documentation.
builder.Services.AddOpenApi();

// Configure the Booking database.
builder.Services.AddDbContext<BookingContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("BookingDatabase")
        ?? throw new InvalidOperationException(
            "BookingDatabase connection string is missing."));
});

// Register application services.
builder.Services.AddScoped<BookingDbSeeder>();

builder.Services.AddScoped<
    IBookingRepository,
    BookingRepository>();

builder.Services.AddScoped<
    IBookingService,
    BookingService>();

builder.Services.AddScoped<
    BookingAccessLogService>();

// Configure Clearance API integration.
builder.Services.AddHttpClient<
    IClearanceNotificationClient,
    ClearanceNotificationClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ClearanceApi"]
        ?? "http://localhost:5165/");

    client.Timeout = TimeSpan.FromSeconds(2);
});

// Configure JWT authentication.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "JWT signing key is missing. Configure Jwt:Key using User Secrets.");

var signingKey = new SymmetricSecurityKey(
    Convert.FromBase64String(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "Vulindlela.Portal",

            ValidateAudience = true,
            ValidAudience = "Vulindlela.Booking.Api",

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,

            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = "unique_name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Create and seed the database on startup.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider
        .GetRequiredService<BookingDbSeeder>();

    await seeder.SeedAsync();
}

// Configure HTTP middleware.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }