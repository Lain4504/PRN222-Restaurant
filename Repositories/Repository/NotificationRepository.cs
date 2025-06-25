using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Repositories.IRepository;

namespace PRN222_Restaurant.Repositories.Repository;

public class NotificationRepository : INotificationRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public NotificationRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Include(n => n.User)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountByUserIdAsync(int userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<PagedResult<Notification>> GetPagedByUserIdAsync(int userId, int page, int pageSize)
    {
        try
        {
            // Console.WriteLine($"NotificationRepository: GetPagedByUserIdAsync - UserId: {userId}, Page: {page}, PageSize: {pageSize}");

            using var context = await _contextFactory.CreateDbContextAsync();

            var query = context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            // Console.WriteLine($"NotificationRepository: Total count for user {userId}: {totalCount}");

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Console.WriteLine($"NotificationRepository: Retrieved {items.Count} items for page {page}");

            return new PagedResult<Notification>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationRepository: Error in GetPagedByUserIdAsync: {ex.Message}");
            throw;
        }
    }

    public async Task AddAsync(Notification notification)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            await context.Notifications.AddAsync(notification);
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            throw new Exception("Lỗi khi thêm Notification: " + innerMessage, dbEx);
        }
    }

    public async Task UpdateAsync(Notification notification)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Notifications.Update(notification);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = await context.Notifications.FindAsync(id);
        if (notification != null)
        {
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAsReadAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = await context.Notifications.FindAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        try
        {
            // Console.WriteLine($"NotificationRepository: MarkAllAsReadAsync - UserId: {userId}");

            using var context = await _contextFactory.CreateDbContextAsync();

            var notifications = await context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            // Console.WriteLine($"NotificationRepository: Found {notifications.Count} unread notifications for user {userId}");

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await context.SaveChangesAsync();
            // Console.WriteLine($"NotificationRepository: Successfully marked {notifications.Count} notifications as read");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NotificationRepository: Error in MarkAllAsReadAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetLatestByUserIdAsync(int userId, int count = 5)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}