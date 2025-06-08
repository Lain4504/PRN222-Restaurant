using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services.IService
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(int id);
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> CreateImmediateOrderAsync(Order order, Dictionary<int, int> selectedItems);
        Task<Order> CreatePreOrderAsync(Order order, Dictionary<int, int> selectedItems);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> CancelOrderAsync(int id);
        Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time);
        Task<List<Table>> GetAvailableTablesAsync();
        Task SendOrderConfirmationEmailAsync(Order order);
    }
} 