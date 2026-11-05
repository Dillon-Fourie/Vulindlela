using Vulindlela.Booking.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Services;
using Vulindlela.Booking.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Enable OpenAPI documentation.
builder.Services.AddOpenApi();

// Configure the Booking service's SQLite database.
builder.Services.AddDbContext<BookingContext>(options =>
{
    options.UseSqlite("Data Source=booking.db");
});

// Register the database seeder with dependency injection.
builder.Services.AddScoped<BookingDbSeeder>();

builder.Services.AddScoped<
    IBookingRepository,
    BookingRepository>();

builder.Services.AddScoped<
    IBookingService,
    BookingService>();

builder.Services.AddHttpClient<
    IClearanceNotificationClient,
    ClearanceNotificationClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ClearanceApi"]
        ?? "http://localhost:5165/");

    client.Timeout = TimeSpan.FromSeconds(2);
});

var app = builder.Build();

// Create and seed the database on application startup.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider
        .GetRequiredService<BookingDbSeeder>();

    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controller endpoints.
app.MapControllers();

app.Run();
