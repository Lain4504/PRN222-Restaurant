using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Helpers;

public class NotificationHelper
{
    private readonly INotificationService _notificationService;

    public NotificationHelper(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task NotifyReservationCreatedAsync(int userId, int reservationId)
    {
        await _notificationService.CreateReservationNotificationAsync(userId, reservationId);
    }

    public async Task NotifyOrderCreatedAsync(int userId, int orderId)
    {
        await _notificationService.CreateOrderNotificationAsync(userId, orderId);
    }

    public async Task NotifyPaymentSuccessAsync(int userId, int orderId, decimal amount)
    {
        await _notificationService.CreatePaymentSuccessNotificationAsync(userId, orderId, amount);
    }

    public async Task NotifyPaymentFailedAsync(int userId, int orderId)
    {
        await _notificationService.CreatePaymentFailedNotificationAsync(userId, orderId);
    }

    public async Task NotifyOrderCompletedAsync(int userId, int orderId)
    {
        await _notificationService.CreateOrderCompletedNotificationAsync(userId, orderId);
    }

    public async Task NotifyOrderAutoCancelledAsync(int userId, int orderId)
    {
        await _notificationService.CreateOrderAutoCancelledNotificationAsync(userId, orderId);
    }

    public async Task NotifyPaymentCompletedAsync(int userId, int orderId, decimal amount)
    {
        await _notificationService.CreatePaymentCompletedNotificationAsync(userId, orderId, amount);
    }
}
