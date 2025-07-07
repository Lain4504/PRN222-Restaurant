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
        private readonly ITableService _tableService;
        private readonly Helpers.NotificationHelper _notificationHelper;

        public OrderService(IOrderRepository orderRepository, ApplicationDbContext context, ITableService tableService, Helpers.NotificationHelper notificationHelper)
        {
            _orderRepository = orderRepository;
            _context = context;
            _tableService = tableService;
            _notificationHelper = notificationHelper;
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

        public async Task<PagedResult<Order>> GetPagedUserOrdersAsync(int userId, int page, int pageSize, string? status = null)
        {
            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId);

            // Nếu có lọc trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Sắp xếp theo thời gian đặt hàng mới nhất
            query = query.OrderByDescending(o => o.OrderDate);

            // Tổng số đơn thỏa điều kiện
            var totalCount = await query.CountAsync();

            // Lấy dữ liệu theo trang
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

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Update table status to Pending when order is created
            if (createdOrder.TableId.HasValue && createdOrder.TableId.Value > 0)
            {
                var table = await _context.Tables.FindAsync(createdOrder.TableId.Value);
                if (table != null)
                {
                    table.Status = "Pending";
                    await _context.SaveChangesAsync();
                }
            }

            return createdOrder;
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

            // Update table status to Pending when order is created
            if (createdOrder.TableId.HasValue && createdOrder.TableId.Value > 0)
            {
                var table = await _context.Tables.FindAsync(createdOrder.TableId.Value);
                if (table != null)
                {
                    table.Status = "Pending";
                    await _context.SaveChangesAsync();
                }
            }

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

        public async Task<int> AutoCancelUnpaidOrdersAsync()
        {
            Console.WriteLine($"[Hangfire] AutoCancelUnpaidOrdersAsync running at {DateTime.Now}");
            var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
            // Lấy các đơn chưa thanh toán, status còn hoạt động, và đã tạo hơn 10 phút
            var unpaidOrders = await _context.Orders
                .Where(o => (o.Status == "Pending" || o.Status == "Preparing")
                    && o.OrderDate <= tenMinutesAgo)
                .ToListAsync();

            int cancelledCount = 0;
            foreach (var order in unpaidOrders)
            {
                // Kiểm tra có payment thành công không
                var hasPaid = await _context.Payments.AnyAsync(p => p.OrderId == order.Id && p.Status == "Paid");
                if (!hasPaid)
                {
                    order.Status = "Cancelled"; // hoặc "AutoCancelled" nếu muốn phân biệt
                    _context.Orders.Update(order);
                    cancelledCount++;

                    // Gửi thông báo cho khách nếu có UserId
                    if (order.UserId.HasValue)
                    {
                        await _notificationHelper.NotifyOrderAutoCancelledAsync(order.UserId.Value, order.Id);
                    }
                    // Cập nhật trạng thái bàn về Available nếu có TableId
                    if (order.TableId.HasValue)
                    {
                        await _tableService.ChangeStatusAsync(order.TableId.Value, "Available");
                    }
                }
            }
            if (cancelledCount > 0)
            {
                await _context.SaveChangesAsync();
            }
            return cancelledCount;
        }
    }
} 