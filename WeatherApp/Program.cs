using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using WeatherApp.Hubs;
using WeatherApp.Models;
using WeatherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Weather_History>();
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddSingleton<WeatherUpdateService>();
builder.Services.AddHostedService(provider => provider.GetService<WeatherUpdateService>());
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<WeatherService>()
    .AddPolicyHandler(GetRetryPolicy()) // Add retry policy
    .AddPolicyHandler(GetTimeoutPolicy()); // Add timeout policy



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

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Timeout policy
static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(10); // 10 seconds timeout
}