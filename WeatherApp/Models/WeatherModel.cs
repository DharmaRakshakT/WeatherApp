using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WeatherApp.Models;

public class WeatherModel
{
    
    public string Location { get; set; }
    public int UpdateInterval { get; set; } // Interval in minutes
    public int NumberOfDays { get; set; } // Number of days for historical data

    public double CurrentTemperature { get; set; }
    public string CurrentDescription { get; set; }
    //public List<WeatherData> Forcast { get; set; } = new List<WeatherData>();
    public Weather_History HistoricalData { get; set; } = new Weather_History();
    public Weather_History Recorded_History { get; set; } = new Weather_History();
}


