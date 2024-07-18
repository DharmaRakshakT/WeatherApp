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
        private readonly string _apiKey = "6f1d314a2c0c4bd703bebb7d5a563785";  //88342dc176b514bb3d1c1667ee63da26
        private readonly ILogger<WeatherService> _logger;
        private readonly Weather_History _weather_History;

        private double Latitude { get; set; }
        private double Longitude { get; set; }
        private string Location { get; set; } = "Tampa";

        public WeatherService(HttpClient httpClient,Weather_History weather_History, ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _weather_History = weather_History;
        }

        public async Task<WeatherModel> GetWeatherDataAsync(string location, int numberOfDays=0 , int numberOfDays_Recorded=0)
        {
            var weatherModel = new WeatherModel { Location = location, NumberOfDays = numberOfDays, History_Days= numberOfDays_Recorded };

            // Get latitude and longitude of the location
            var (lat, lon) = await GetCoordinatesAsync(location);

            // Get current weather data
            var currentWeather = await GetCurrentWeatherAsync(lat, lon);
            weatherModel.CurrentTemperature = currentWeather.Item1;
            weatherModel.CurrentDescription = currentWeather.Item2;

            //Weather_History weather_History = new Weather_History();
            //weather_History.Records=await GetHistoricalWeatherDataAsync(lat, lon,numberOfDays);

            // Get weather data
            var weatherData = await GetWeatherDataAsync(lat, lon);
            DateTime dateTime = DateTime.Now;
            var record = new WeatherData { Timestamp = dateTime, Temperature = currentWeather.Item1, Description = currentWeather.Item2, Date=dateTime.Date, WindSpeed= currentWeather.Item3 ,City=location};

            _weather_History.Records.Add(record);

            weatherModel.CurrentDate = DateTime.Now;
            weatherModel.Recorded_History.Records = _weather_History.GetRecordsByCity(location,numberOfDays_Recorded);
            // Filter weather data by date
            weatherModel.ForecastedData.Records = FilterWeatherData(weatherData, numberOfDays);

            //
            var historicalData = await GetHistoricalWeatherDataAsync(lat, lon, numberOfDays_Recorded);
            weatherModel.HistoricalData.Records = historicalData;

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

        private async Task<(double, string,double)> GetCurrentWeatherAsync(double lat, double lon)
        {
            string currentWeatherApiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
            string response = await _httpClient.GetStringAsync(currentWeatherApiUrl);
            JObject data = JObject.Parse(response);
            double temperature = data["main"]["temp"].Value<double>();
            double windspeed= data["wind"]["speed"].Value<double>();
            string description = data["weather"][0]["description"].Value<string>();
            return (temperature, description,windspeed);
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
            return FilterWeatherData_History((JArray)weatherData["list"], numberOfDays);


        }
        private List<WeatherData> FilterWeatherData_History(JArray weatherData, int numberOfDays)
        {
            var filteredData = new List<WeatherData>();

            if (weatherData == null || weatherData.Count == 0)
            {
                _logger.LogWarning("Weather data is null or empty.");
                return filteredData;
            }

            // Group data by date
            var groupedData = weatherData
                .GroupBy(entry => DateTimeOffset.FromUnixTimeSeconds(entry["dt"].Value<long>()).Date)
                .OrderByDescending(group => group.Key) // Order by date descending to get the latest days first
                .Take(numberOfDays); // Take only the required number of days

            foreach (var group in groupedData)
            {
                var firstEntry = group.FirstOrDefault();
                if (firstEntry == null)
                {
                    _logger.LogWarning("First entry in group is null.");
                    continue;
                }

                var date = firstEntry["dt"].Value<long>();
                var temperature = firstEntry["main"]?["temp"]?.Value<double>();
                var description = firstEntry["weather"]?[0]?["description"]?.Value<string>();
                var windSpeed = firstEntry["wind"]?["speed"]?.Value<double>();

                if (temperature == null || description == null || windSpeed == null)
                {
                    _logger.LogWarning("One or more weather data fields are null.");
                    continue;
                }

                filteredData.Add(new WeatherData
                {
                    Date = DateTimeOffset.FromUnixTimeSeconds(date).DateTime,
                    Temperature = temperature.Value,
                    Description = description,
                    WindSpeed = windSpeed.Value
                });
            }

            return filteredData;
        }

    }
}
