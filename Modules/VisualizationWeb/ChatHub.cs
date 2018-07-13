using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    public class VisualizerHub : Hub
    {
        // Called when one client tries to send a message. It will then broadcast
        // To all clients with that data.
        public async Task SendInput(String input)
        {
            await Clients.All.SendAsync("ReceiveInput", input);
        }
    }
}