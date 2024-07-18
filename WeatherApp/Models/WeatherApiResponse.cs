public class Weather_Data
{
    public int Dt { get; set; }
    public Main Main { get; set; }
    public Wind Wind { get; set; }
    public List<Weather> Weather { get; set; }
}

public class Main
{
    public float Temp { get; set; }
    
}

public class Wind
{
    public float Speed { get; set; }
}



public class Weather
{
    public int Id { get; set; }
    public string Main { get; set; }
    public string Description { get; set; }

}

public class WeatherApiResponse
{
    public List<Weather_Data> List { get; set; }
}
