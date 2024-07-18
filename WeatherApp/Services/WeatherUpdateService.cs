using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WeatherApp.Hubs;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class WeatherUpdateService : BackgroundService
    {
        private readonly WeatherService _weatherService;
        private readonly IHubContext<WeatherHub> _hubContext;
        private readonly ILogger<WeatherUpdateService> _logger;
        private WeatherModel _weatherModel;
        private CancellationTokenSource _cancellationTokenSource;

        public WeatherUpdateService(WeatherService weatherService, IHubContext<WeatherHub> hubContext, ILogger<WeatherUpdateService> logger)
        {
            _weatherService = weatherService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public void ConfigureWeatherModel(WeatherModel weatherModel)
        {
            _logger.LogInformation("Configuring weather model.");
            _weatherModel = weatherModel;
            _cancellationTokenSource?.Cancel();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_weatherModel != null)
                {
                    try
                    {
                        _logger.LogInformation("Fetching updated weather data.");
                        var updatedWeatherModel = await _weatherService.GetWeatherDataAsync(_weatherModel.Location, _weatherModel.NumberOfDays,_weatherModel.History_Days);
                        
                        _weatherModel.CurrentTemperature = updatedWeatherModel.CurrentTemperature;
                        _weatherModel.CurrentDescription = updatedWeatherModel.CurrentDescription;
                        _weatherModel.ForecastedData = updatedWeatherModel.ForecastedData;
                        _weatherModel.Recorded_History = updatedWeatherModel.Recorded_History;
                        _weatherModel.CurrentDate = updatedWeatherModel.CurrentDate;
                        //_weatherModel.Forcast = updatedWeatherModel.Forcast;

                        await _hubContext.Clients.All.SendAsync("ReceiveWeatherUpdate", _weatherModel);
                        _logger.LogInformation("Weather data updated and broadcasted successfully.");

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while updating weather.");
                    }

                    // Create a new CancellationTokenSource and delay
                    _cancellationTokenSource = new CancellationTokenSource();
                    try
                    {
                        await Task.Delay(_weatherModel.UpdateInterval * 60 * 1000, _cancellationTokenSource.Token); // Delay based on the update interval
                    }
                    catch (TaskCanceledException)
                    {
                        // Task was cancelled, no action needed
                    }
                }
                else
                {
                    // Default delay if _weatherModel is null
                    await Task.Delay(60000, stoppingToken); // Default to 1 minute if no interval set
                }
            }
            
        }
        public override void Dispose()
        {
            _logger.LogInformation("WeatherUpdateService is stopping.");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            base.Dispose();
        }
    }
}
