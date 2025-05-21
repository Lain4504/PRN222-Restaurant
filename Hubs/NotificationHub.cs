using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendNotification(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", user, message);
        }

        public async Task SendOrderNotification(string orderId, string status, string message)
        {
            await Clients.All.SendAsync("ReceiveOrderUpdate", orderId, status, message);
        }

        public async Task SendReservationNotification(string reservationId, string status, string message)
        {
            await Clients.All.SendAsync("ReceiveReservationUpdate", reservationId, status, message);
        }

        public async Task SendToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceivePrivateNotification", message);
        }

        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveGroupNotification", message);
        }
    }
} 