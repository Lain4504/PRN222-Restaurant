using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;

        public OrderService(IOrderRepository orderRepository, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _context = context;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId)
        {
            return await _orderRepository.GetByUserIdAsync(userId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<PagedResult<Order>> GetPagedOrdersAsync(int page, int pageSize)
        {
            return await _orderRepository.GetPagedAsync(page, pageSize);
        }

        public async Task<PagedResult<Order>> GetPagedUserOrdersAsync(int userId, int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Order> CreateImmediateOrderAsync(Order order, Dictionary<int, int> selectedItems)
        {
            order.OrderType = "Immediate";
            order.OrderDate = DateTime.Now;
            order.Status = "Pending";

            // Add order items and calculate total price
            decimal totalPrice = 0;
            foreach (var item in selectedItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.Key);
                if (menuItem != null)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        MenuItemId = item.Key,
                        Quantity = item.Value,
                        UnitPrice = menuItem.Price
                    });
                    totalPrice += menuItem.Price * item.Value;
                }
            }
            order.TotalPrice = totalPrice;

            return await _orderRepository.CreateAsync(order);
        }

        public async Task<Order> CreatePreOrderAsync(Order order, Dictionary<int, int> selectedItems)
        {
            // Initialize OrderItems collection if null
            if (order.OrderItems == null)
            {
                order.OrderItems = new List<OrderItem>();
            }
            
            order.OrderType = "PreOrder";
            order.OrderDate = DateTime.Now;
            order.Status = "Pending";
            
            Console.WriteLine($"Creating pre-order for table {order.TableId} with {selectedItems.Count} items");

            // Add order items and calculate total price
            decimal totalPrice = 0;
            foreach (var item in selectedItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.Key);
                if (menuItem != null)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        MenuItemId = item.Key,
                        Quantity = item.Value,
                        UnitPrice = menuItem.Price
                    });
                    totalPrice += menuItem.Price * item.Value;
                    Console.WriteLine($"Added item {menuItem.Name} x{item.Value} at ${menuItem.Price} each");
                }
            }
            order.TotalPrice = totalPrice;
            Console.WriteLine($"Total order price: ${totalPrice}");

            var createdOrder = await _orderRepository.CreateAsync(order);
            Console.WriteLine($"Created order with ID: {createdOrder.Id}");
            
            await SendOrderConfirmationEmailAsync(createdOrder);
            return createdOrder;
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            order.Status = status;
            return await _orderRepository.UpdateAsync(order);
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            return await UpdateOrderStatusAsync(id, "Cancelled");
        }

        public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time)
        {
            return await _orderRepository.IsTableAvailableAsync(tableId, date, time);
        }

        public async Task<List<Table>> GetAvailableTablesAsync()
        {
            return await _orderRepository.GetAvailableTablesAsync();
        }

        public async Task SendOrderConfirmationEmailAsync(Order order)
        {
            // TODO: Implement email sending logic
            // This would typically use a service like SendGrid or SMTP
            await Task.CompletedTask;
        }
        
        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        
        // Interface implementation for IOrderService
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _orderRepository.GetAllAsync();
        }
        
        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _orderRepository.GetByIdAsync(id);
        }
        
        public async Task<Order> CreateAsync(Order order)
        {
            return await _orderRepository.CreateAsync(order);
        }
        
        public async Task UpdateAsync(Order order)
        {
            await _orderRepository.UpdateAsync(order);
        }
        
        public async Task DeleteAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
        }
    }
} 