using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.Hubs
{
    public class WeatherHub : Hub
    {
        public async Task SendWeatherUpdate(WeatherModel weatherModel)
        {
            await Clients.All.SendAsync("ReceiveWeatherUpdate", weatherModel);
        }
    }
}
