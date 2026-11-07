using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Portal.Data;
using Vulindlela.Portal.Models.Auth;
using Vulindlela.Portal.Authorization;
using Microsoft.AspNetCore.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Add MVC services.
builder.Services.AddControllersWithViews();

// HTTP client for the Booking API.
builder.Services.AddHttpClient("BookingApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:BookingApi"]
        ?? "http://localhost:5112/");

    client.Timeout = TimeSpan.FromSeconds(5);
});

// HTTP client for the Clearance API.
builder.Services.AddHttpClient("ClearanceApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ClearanceApi"]
        ?? "http://localhost:5165/");

    client.Timeout = TimeSpan.FromSeconds(2);
});

builder.Services.AddScoped<
    Vulindlela.Portal.Services.AppointmentPortalService>();

builder.Services.AddScoped<
    Vulindlela.Portal.Services.AccessLogService>();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("AuthDatabase")
        ?? throw new InvalidOperationException(
            "AuthDatabase connection string is missing.")));

builder.Services.AddScoped<IPasswordHasher<ApplicationUser>,
    PasswordHasher<ApplicationUser>>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "Vulindlela.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CitizenRecordAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new CitizenRecordRequirement());
    });
});

builder.Services.AddScoped<
    IAuthorizationHandler,
    CitizenRecordAuthorizationHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    db.Database.Migrate();

    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<ApplicationUser>>();

    await AuthSeeder.SeedAsync(db, passwordHasher);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
