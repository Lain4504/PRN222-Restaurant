using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;

namespace PRN222_Restaurant.Repositories.IRepository;

public interface INotificationRepository
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
}