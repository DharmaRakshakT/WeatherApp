using WeatherApp.Hubs;
using WeatherApp.Models;
using WeatherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Weather_History>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddSingleton<WeatherUpdateService>();
builder.Services.AddHostedService(provider => provider.GetService<WeatherUpdateService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Weather}/{action=Index}/{id?}");

app.MapHub<WeatherHub>("/weatherHub");

app.Run();
