using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WeatherApp.Hubs;
using WeatherApp.Models;
using WeatherApp.Services;
using System;

namespace WeatherApp.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherService _weatherService;
        private readonly WeatherUpdateService _weatherUpdateService;
        private readonly IHubContext<WeatherHub> _hubContext;
        private static WeatherModel _weatherModel;
        private readonly Weather_History _weather_History;

        public WeatherController(WeatherService weatherService, WeatherUpdateService weatherUpdateService, IHubContext<WeatherHub> hubContext, Weather_History weather_History)
        {
            _weatherService = weatherService;
            _weatherUpdateService = weatherUpdateService;
            _hubContext = hubContext;
            _weather_History = weather_History;
        }

        public IActionResult Index()
        {
            return View(_weatherModel);
        }

        [HttpPost]
        public async Task<IActionResult> SetWeatherConfig(WeatherModel weatherModel)
        {
            try
            {
                // Server-side validation
                if (string.IsNullOrEmpty(weatherModel.Location))
                {
                    throw new ArgumentException("Location cannot be empty.");
                }
                if (weatherModel.UpdateInterval <= 0)
                {
                    throw new ArgumentException("Update interval must be a positive integer greater than 0.");
                }
                if (weatherModel.History_Days < 0)
                {
                    throw new ArgumentException("Number of Days for Recorded Historical Data must be a valid number and cannot be negative.");
                }
                if (weatherModel.NumberOfDays < 0)
                {
                    throw new ArgumentException("Number of Days for Forecast Data must be a valid number and cannot be negative.");
                }

                // Proceed with the rest of the method if validation passes
                 _weatherModel = await _weatherService.GetWeatherDataAsync(weatherModel.Location, weatherModel.NumberOfDays, weatherModel.History_Days);
                _weatherModel.UpdateInterval = weatherModel.UpdateInterval;
                _weatherModel.NumberOfDays = weatherModel.NumberOfDays;
                _weatherModel.History_Days = weatherModel.History_Days;
                _weatherModel.Location = weatherModel.Location;
                //_weatherModel.Recorded_History.Records = _weather_History.GetRecordsByCity(location, numberOfDays_recorded);

                _weatherUpdateService.ConfigureWeatherModel(_weatherModel);

                await _hubContext.Clients.All.SendAsync("ReceiveWeatherUpdate", _weatherModel);

                TempData["SuccessMessage"] = "Weather configuration updated successfully.";
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
