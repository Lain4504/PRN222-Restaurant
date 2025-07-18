using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;

namespace PRN222_Restaurant.Services.IService
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<List<Order>> GetAllAsync();
        Task<PagedResult<Order>> GetPagedAsync(int page, int pageSize);
        Task<Order> CreateAsync(Order order);
        Task<bool> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time);
        Task<List<Table>> GetAvailableTablesAsync();
    }
} 