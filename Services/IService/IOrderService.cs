using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Services.IService
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(int id);
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<PagedResult<Order>> GetPagedOrdersAsync(int page, int pageSize);
        Task<PagedResult<Order>> GetPagedUserOrdersAsync(int userId, int page, int pageSize, string status);
        Task<Order> CreateImmediateOrderAsync(Order order, Dictionary<int, int> selectedItems);
        Task<Order> CreatePreOrderAsync(Order order, Dictionary<int, int> selectedItems);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> CancelOrderAsync(int id);
        Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time);
        Task<List<Table>> GetAvailableTablesAsync();
        Task SendOrderConfirmationEmailAsync(Order order);
        Task<Order?> GetOrderWithItemsAsync(int id);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<Order> CreateAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
    }
} 