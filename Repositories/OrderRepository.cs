using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Repositories
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
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Order>> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
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
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<Order> CreateAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
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
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => 
                    o.TableId == tableId && 
                    o.OrderDate.Date == date.Date &&
                    o.ReservationTime == time &&
                    o.Status != "Cancelled");

            return existingOrder == null;
        }

        public async Task<List<Table>> GetAvailableTablesAsync()
        {
            return await _context.Tables
                .Where(t => t.Status == "Available")
                .ToListAsync();
        }
    }
} 