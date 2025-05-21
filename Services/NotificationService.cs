using Microsoft.AspNetCore.SignalR;
using PRN222_Restaurant.Hubs;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Services
{
    public interface INotificationService
    {
        Task SendOrderNotification(string orderId, string status, string message);
        Task SendReservationNotification(string reservationId, string status, string message);
        Task SendUserNotification(string userId, string message);
        Task SendGroupNotification(string groupName, string message);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendOrderNotification(string orderId, string status, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", orderId, status, message);
            
            // Send to specific group (e.g., staff, kitchen)
            await _hubContext.Clients.Group("staff").SendAsync("ReceiveOrderUpdate", orderId, status, message);
            await _hubContext.Clients.Group("kitchen").SendAsync("ReceiveOrderUpdate", orderId, status, message);
        }

        public async Task SendReservationNotification(string reservationId, string status, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveReservationUpdate", reservationId, status, message);
            
            // Send to staff group
            await _hubContext.Clients.Group("staff").SendAsync("ReceiveReservationUpdate", reservationId, status, message);
        }

        public async Task SendUserNotification(string userId, string message)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceivePrivateNotification", message);
        }

        public async Task SendGroupNotification(string groupName, string message)
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveGroupNotification", message);
        }
    }
} 