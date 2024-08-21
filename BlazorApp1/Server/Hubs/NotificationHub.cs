using BlazorApp1.Server.Test;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;


namespace BlazorApp1.Server.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly AppInitializationService _app;

        public NotificationHub(AppInitializationService app)
        {
            _app = app;
        }

      
        public async Task SendNotification(int dni, bool hasAccess)
        {
            await Clients.All.SendAsync("SendNotification", dni, hasAccess);
        }
        public async Task COMChanges(string serialPort)
        {
            Console.WriteLine("HOLA");

            await _app.StopAsync(cancellationToken:System.Threading.CancellationToken.None);
            await _app.Start(serialPort);
            
            await Clients.All.SendAsync("COMChanges", serialPort);
        }
    }
}

