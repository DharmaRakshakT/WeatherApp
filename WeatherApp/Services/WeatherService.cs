using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;
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
            _logger.LogInformation("Fetching weather data for location: {Location}", location);

            var weatherModel = new WeatherModel { Location = location, Historical_Data_Days = numberOfDays, Recorded_History_Days= numberOfDays_Recorded };

            try
            {
                // Get latitude and longitude of the location
                var (lat, lon) = await GetCoordinatesAsync(location);

                // Get current weather data
                var currentWeather = await GetCurrentWeatherAsync(lat, lon);
                weatherModel.CurrentTemperature = currentWeather.Item1;
                currentWeather.Item2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(currentWeather.Item2.ToLower());
                weatherModel.CurrentDescription = currentWeather.Item2;

                

                // Get weather data
                DateTime dateTime = DateTime.Now;
                var record = new WeatherData { Timestamp = dateTime, Temperature = currentWeather.Item1, Description = currentWeather.Item2, Date = dateTime.Date, WindSpeed = currentWeather.Item3, City = location };

                _weather_History.Records.Add(record);

                weatherModel.CurrentDate = DateTime.Now;
                weatherModel.Recorded_History.Records = _weather_History.GetRecordsByCity(location, numberOfDays_Recorded);

                weatherModel.Historical_data.Records = await GetHistoricalWeatherDataAsync(lat, lon, numberOfDays, location);

                _logger.LogInformation("Weather data fetched successfully for location: {Location}", location);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data for location: {Location}", location);
                throw;
            }

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



        private async Task<List<WeatherData>> GetHistoricalWeatherDataAsync(double lat, double lon, int numberOfDays,string location)
        {
            
            DateTime dateTime = DateTime.Now;
            DateTime startDate = dateTime.AddDays(-numberOfDays); // Replace with your start date
            DateTime endDate = dateTime.Date; // Replace with your end date

            List<Weather_Data> allWeatherData = new List<Weather_Data>();

            using (HttpClient client = new HttpClient())
            {
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    long unixTimestamp = ((DateTimeOffset)date).ToUnixTimeSeconds();
                    string url = $"https://history.openweathermap.org/data/2.5/history/city?lat={lat}&lon={lon}&type=hour&start={unixTimestamp}&end={unixTimestamp + 86400}&appid={_apiKey}&units=metric";
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    WeatherApiResponse weatherResponse = JsonConvert.DeserializeObject<WeatherApiResponse>(responseBody);
                    allWeatherData.AddRange(weatherResponse.List);
                }
            }

            var dailySummaries = allWeatherData
                .GroupBy(w => DateTimeOffset.FromUnixTimeSeconds(w.Dt).Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TempAvg = Math.Round(g.Average(w => w.Main.Temp), 2),
                    WindSpeedAvg = Math.Round(g.Average(w => w.Wind.Speed), 2),
                    WeatherDescription = g.First().Weather.First().Description
                });
            List<WeatherData> weatherData =  new List<WeatherData>();

            foreach (var summary in dailySummaries)
            {
                DateTime dateTime1 = DateTime.Now;
                if (summary.Date != dateTime1.Date)
                {
                    WeatherData weather = new WeatherData();
                    weather.Date = summary.Date;
                    weather.Temperature = Math.Round(summary.TempAvg, 2);
                    weather.Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(summary.WeatherDescription.ToLower());
                    weather.WindSpeed = Math.Round(summary.WindSpeedAvg, 2);
                    weather.City = location;
                    weatherData.Add(weather);
                }
               
            }
            weatherData= weatherData.OrderByDescending(record => record.Date).ToList();
            return weatherData;
        }

    }
}
