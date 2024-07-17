namespace WeatherApp.Models
{
    public class Weather_History
    {
        public List<WeatherData> Records { get; set; } = new List<WeatherData>();

    }
    public class WeatherData
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
        public double WindSpeed { get; set; }
    }
}
