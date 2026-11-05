using Vulindlela.Booking.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Services;

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
