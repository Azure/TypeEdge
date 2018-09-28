using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs
{
    public class VisualizerHub : Hub
    {
        // Called when one client tries to send a message. It will then broadcast
        // To all clients with that data.
        public async Task SendInput(string message)
        {
            await Clients.All.SendAsync("ReceiveInput", message);
        }
    }
}