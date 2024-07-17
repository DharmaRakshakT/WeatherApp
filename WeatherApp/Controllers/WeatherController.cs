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
        public async Task<IActionResult> SetWeatherConfig(WeatherModel weatherModel,string location, int updateInterval, int numberOfDays , int numberOfDays_recorded)
        {
            try
            {
                
                _weatherModel = await _weatherService.GetWeatherDataAsync(location, numberOfDays,numberOfDays_recorded);
                _weatherModel.UpdateInterval = updateInterval;
                _weatherModel.NumberOfDays = numberOfDays;
                _weatherModel.History_Days = numberOfDays_recorded;
                _weatherModel.Location = location;
                //_weatherModel.Recorded_History.Records = _weather_History.GetRecordsByCity(location,numberOfDays_recorded);
            
                
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
