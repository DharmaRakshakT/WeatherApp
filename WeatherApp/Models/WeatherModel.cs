using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WeatherApp.Models;

public class WeatherModel
{
    public double CurrentTemperature { get; set; }
    public string CurrentDescription { get; set; }
    //public List<WeatherData> Forcast { get; set; } = new List<WeatherData>();
    public List<WeatherData> HistoricalData { get; set; } = new List<WeatherData>();
    public string Location { get; set; }
    public int UpdateInterval { get; set; } // Interval in minutes
    public int NumberOfDays { get; set; } // Number of days for historical data
}

public class WeatherData
{
    public DateTime Date { get; set; }
    public double Temperature { get; set; }
    public string Description { get; set; }
    public double WindSpeed { get; set; }
}
