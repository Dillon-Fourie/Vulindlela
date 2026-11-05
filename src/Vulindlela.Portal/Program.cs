
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
