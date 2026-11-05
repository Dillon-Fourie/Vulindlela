
using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data;
using Vulindlela.Clearance.Api.Repositories;
using Vulindlela.Clearance.Api.Services;
using Vulindlela.Clearance.Api.BackgroundTasks;

var builder = WebApplication.CreateBuilder(args);

// Register API controllers.
builder.Services.AddControllers();

// Enable OpenAPI documentation.
builder.Services.AddOpenApi();

// Register the Clearance service's own SQLite database.
// The Booking service must never access this database.
builder.Services.AddDbContext<ClearanceContext>(options =>
    options.UseSqlite("Data Source=clearance.db"));

    builder.Services.AddScoped<
    IClearanceRepository,
    ClearanceRepository>();

    builder.Services.AddScoped<
    IDocumentVerificationService,
    DocumentVerificationService>();

    builder.Services.AddSingleton<
    IClearanceQueue,
    ClearanceQueue>();

    builder.Services.AddHostedService<ClearanceBackgroundWorker>();

var app = builder.Build();

// Create the Clearance database and tables on startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ClearanceContext>();

    await dbContext.Database.EnsureCreatedAsync();
}

// Enable OpenAPI in development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable controller endpoints.
app.MapControllers();

app.Run();
