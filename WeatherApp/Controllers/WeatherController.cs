using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WeatherApp.Hubs;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherService _weatherService;
        private readonly WeatherUpdateService _weatherUpdateService;
        private readonly IHubContext<WeatherHub> _hubContext;
        private static WeatherModel _weatherModel;

        public WeatherController(WeatherService weatherService, WeatherUpdateService weatherUpdateService, IHubContext<WeatherHub> hubContext)
        {
            _weatherService = weatherService;
            _weatherUpdateService = weatherUpdateService;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View(_weatherModel);
        }

        [HttpPost]
        public async Task<IActionResult> SetWeatherConfig(string location, int updateInterval, int numberOfDays)
        {
            try
            {
                _weatherModel = await _weatherService.GetWeatherDataAsync(location, numberOfDays);
                _weatherModel.UpdateInterval = updateInterval;
                _weatherModel.NumberOfDays = numberOfDays;
                _weatherModel.Location = location;
                _weatherUpdateService.ConfigureWeatherModel(_weatherModel);

                await _hubContext.Clients.All.SendAsync("ReceiveWeatherUpdate", _weatherModel);

                TempData["SuccessMessage"] = "Weather configuration updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
