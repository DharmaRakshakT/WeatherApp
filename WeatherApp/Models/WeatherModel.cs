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
    public int Historical_Data_Days { get; set; } // Number of days for historical data
    public double CurrentTemperature { get; set; }
    public string CurrentDescription { get; set; }
    public int Recorded_History_Days { get; set; }
    public DateTime CurrentDate { get; set; }
    public Weather_History Recorded_History { get; set; } = new Weather_History();
    public Weather_History Historical_data { get; set; } = new Weather_History();


}


