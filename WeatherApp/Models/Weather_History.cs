namespace WeatherApp.Models
{
    public class Weather_History
    {
        
        public List<WeatherData> Records { get; set; } = new List<WeatherData>();
        public List<WeatherData> GetRecordsByCity(string city, int numberOfDays = 0)
        {
            if (numberOfDays == 0)
            {

                return Records
                    .Where(record => record.City.Equals(city, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(record => record.Timestamp)
                    .ToList();
            }
            else
            {
                DateTime startDate = DateTime.Now.AddDays(-numberOfDays);
                return Records
                .Where(record => record.City.Equals(city, StringComparison.OrdinalIgnoreCase) && record.Timestamp >= startDate)
                .OrderByDescending(record => record.Timestamp)
                .ToList();
            }
        }
        public List<WeatherData> OrderRecords()
        {
            return Records.OrderByDescending(record => record.Date).ToList();
        }

    }
    public class WeatherData
    {
        public DateTime Date { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
        public double WindSpeed { get; set; }
        public DateTime Timestamp { get; set; }
        public string City { get; set; }
    }
    
}
