using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WeatherApp.Models;

namespace WeatherApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "88342dc176b514bb3d1c1667ee63da26";  //
        private readonly ILogger<WeatherService> _logger;

        private double Latitude { get; set; }
        private double Longitude { get; set; }
        private string Location { get; set; } = "Tampa";

        public WeatherService(HttpClient httpClient, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<WeatherModel> GetWeatherDataAsync(string location, int numberOfDays)
        {
            var weatherModel = new WeatherModel { Location = location, NumberOfDays = numberOfDays };

            // Get latitude and longitude of the location
            var (lat, lon) = await GetCoordinatesAsync(location);

            // Get current weather data
            var currentWeather = await GetCurrentWeatherAsync(lat, lon);
            weatherModel.CurrentTemperature = currentWeather.Item1;
            weatherModel.CurrentDescription = currentWeather.Item2;

            // Get weather data
            var weatherData = await GetWeatherDataAsync(lat, lon);

            // Filter weather data by date
            weatherModel.HistoricalData = FilterWeatherData(weatherData, numberOfDays);

            //var historicalData = await GetHistoricalWeatherDataAsync(lat, lon, numberOfDays);
            //weatherModel.HistoricalData = historicalData;

            return weatherModel;
        }

        private async Task<(double, double)> GetCoordinatesAsync(string location)
        {
            //var response = await _httpClient.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={location}&limit=1&appid={_apiKey}");
            //var data = JArray.Parse(response);

            string geoApiUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={location}&limit=1&appid={_apiKey}";
            string response = await _httpClient.GetStringAsync(geoApiUrl);
            JArray data = JArray.Parse(response);
            if (data.Count > 0)
            {
                double lat = data[0]["lat"].Value<double>();
                double lon = data[0]["lon"].Value<double>();
                return (lat, lon);
            }
            else
            {
                throw new Exception("Location not found");
            }
        }

        private async Task<(double, string)> GetCurrentWeatherAsync(double lat, double lon)
        {
            string currentWeatherApiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
            string response = await _httpClient.GetStringAsync(currentWeatherApiUrl);
            JObject data = JObject.Parse(response);
            double temperature = data["main"]["temp"].Value<double>();
            string description = data["weather"][0]["description"].Value<string>();
            return (temperature, description);
        }

        private async Task<JArray> GetWeatherDataAsync(double lat, double lon)
        {
            string weatherApiUrl = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
            string response = await _httpClient.GetStringAsync(weatherApiUrl);
            JObject weatherData = JObject.Parse(response);
            return (JArray)weatherData["list"];
        }

        private List<WeatherData> FilterWeatherData(JArray weatherData, int numberOfDays)
        {
            var filteredData = new List<WeatherData>();
            var groupedData = weatherData.GroupBy(entry => DateTime.Parse(entry["dt_txt"].Value<string>()).Date)
                                         .Take(numberOfDays);

            foreach (var group in groupedData)
            {
                var firstEntry = group.First();
                filteredData.Add(new WeatherData
                {
                    Date = DateTime.Parse(firstEntry["dt_txt"].Value<string>()),
                    Temperature = firstEntry["main"]["temp"].Value<double>(),
                    Description = firstEntry["weather"][0]["description"].Value<string>(),
                    WindSpeed = firstEntry["wind"]["speed"].Value<double>()
                });
            }

            return filteredData;
        }

        private async Task<List<WeatherData>> GetHistoricalWeatherDataAsync(double lat, double lon, int numberOfDays)
        {
            string weatherApiUrl = $"https://history.openweathermap.org/data/2.5/history/city?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
            string response = await _httpClient.GetStringAsync(weatherApiUrl);
            JObject weatherData = JObject.Parse(response);
            var weatherDataList = (JArray)weatherData["list"];
            return FilterWeatherData(weatherDataList, numberOfDays);
        }
    }
}
