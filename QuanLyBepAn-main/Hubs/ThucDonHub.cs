using Microsoft.AspNetCore.SignalR;

namespace QuanLyBepAn.Hubs
{
    public class ThucDonHub : Hub
    {
        // Called when menu data changes; server will broadcast to all connected clients
        public async Task NotifyMenuUpdated(object payload)
        {
            await Clients.All.SendAsync("MenuUpdated", payload);
        }
    }
}
