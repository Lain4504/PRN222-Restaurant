using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Table)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Order>> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.MenuItem)
                .Include(o => o.Table)
                .Include(o => o.User)
                .Include(o => o.Payments)
                .OrderByDescending(o => o.OrderDate);

            // Count total items before applying pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create and return paged result
            return new PagedResult<Order>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<Order> CreateAsync(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Successfully saved order {order.Id} with {order.OrderItems?.Count ?? 0} items");
                return order;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to allow the service to handle it
            }
        }

        public async Task<bool> UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time)
        {
            var reservationTime = date.Date.Add(time);
            return !await _context.Orders
                .AnyAsync(o => o.TableId == tableId &&
                             o.OrderType == "PreOrder" &&
                             o.Status != "Cancelled" &&
                             o.ReservationTime == reservationTime);
        }

        public async Task<List<Table>> GetAvailableTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.Status == "Available")
                .ToListAsync();
        }
    }
} 