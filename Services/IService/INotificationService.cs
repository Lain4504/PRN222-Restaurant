using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;

namespace PRN222_Restaurant.Services.IService;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetAllAsync();
    Task<Notification?> GetByIdAsync(int id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
    Task<int> GetUnreadCountByUserIdAsync(int userId);
    Task<PagedResult<Notification>> GetPagedByUserIdAsync(int userId, int page, int pageSize);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task DeleteAsync(int id);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(int userId);
    Task<IEnumerable<Notification>> GetLatestByUserIdAsync(int userId, int count = 5);

    // Business logic methods
    Task CreateReservationNotificationAsync(int userId, int reservationId);
    Task CreateOrderNotificationAsync(int userId, int orderId);
    Task CreatePaymentSuccessNotificationAsync(int userId, int orderId, decimal amount);
    Task CreatePaymentFailedNotificationAsync(int userId, int orderId);
    Task CreateOrderCompletedNotificationAsync(int userId, int orderId);

    // Points-related notifications
    Task CreatePointsEarnedNotificationAsync(int userId, int points, int orderId);
    Task CreatePointsRedeemedNotificationAsync(int userId, int points, decimal discount, int orderId);
    Task CreateWelcomeBonusNotificationAsync(int userId, int points);
}