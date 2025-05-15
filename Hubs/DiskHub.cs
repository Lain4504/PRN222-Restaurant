using Microsoft.AspNetCore.SignalR;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Hubs
{
    public class DiskHub : Hub
    {
        public async Task SendDiskNotification(string message, Disk disk)
        {
            await Clients.All.SendAsync("ReceiveDiskNotification", message, disk);
        }
    }
} 