using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service;

public class PointsService : IPointsService
{
    private readonly ApplicationDbContext _context;
    private readonly PointsConfig _config;
    private readonly INotificationService _notificationService;

    public PointsService(ApplicationDbContext context, IOptions<PointsConfig> config, INotificationService notificationService)
    {
        _context = context;
        _config = config.Value;
        _notificationService = notificationService;
    }

    public async Task<int> CalculatePointsEarnedAsync(decimal orderAmount)
    {
        if (orderAmount < _config.MinimumOrderAmount)
            return 0;

        return (int)Math.Floor(orderAmount * _config.PointsPerDollar);
    }

    public async Task<bool> AwardPointsAsync(int userId, int orderId, decimal orderAmount, string description = "Order payment")
    {
        try
        {
            var pointsEarned = await CalculatePointsEarnedAsync(orderAmount);
            
            if (pointsEarned <= 0)
                return true; // No points to award, but not an error

            // Check if points already awarded for this order
            var existingTransaction = await _context.PointTransactions
                .FirstOrDefaultAsync(pt => pt.UserId == userId && pt.RelatedOrderId == orderId && pt.Type == "Earned");
            
            if (existingTransaction != null)
                return true; // Already awarded

            // Create point transaction
            var transaction = new PointTransaction
            {
                UserId = userId,
                Points = pointsEarned,
                Type = "Earned",
                Description = $"{description} - Order #{orderId}",
                RelatedOrderId = orderId,
                CreatedAt = DateTime.Now,
                ExpiresAt = _config.PointsExpirationDays > 0 ? DateTime.Now.AddDays(_config.PointsExpirationDays) : null
            };

            _context.PointTransactions.Add(transaction);

            // Update user's point balance
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Points += pointsEarned;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            // Send notification about points earned
            await _notificationService.CreatePointsEarnedNotificationAsync(userId, pointsEarned, orderId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<decimal> RedeemPointsAsync(int userId, int pointsToRedeem, int orderId, string description = "Points redemption")
    {
        try
        {
            if (!await ValidatePointsRedemptionAsync(userId, pointsToRedeem))
                return 0;

            var discountAmount = await CalculatePointsDiscountAsync(pointsToRedeem);

            // Create redemption transaction
            var transaction = new PointTransaction
            {
                UserId = userId,
                Points = -pointsToRedeem, // Negative for spending
                Type = "Redeemed",
                Description = $"{description} - Order #{orderId}",
                RelatedOrderId = orderId,
                CreatedAt = DateTime.Now
            };

            _context.PointTransactions.Add(transaction);

            // Update user's point balance
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Points -= pointsToRedeem;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            // Send notification about points redeemed
            await _notificationService.CreatePointsRedeemedNotificationAsync(userId, pointsToRedeem, discountAmount, orderId);

            return discountAmount;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> GetUserPointsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.Points ?? 0;
    }

    public async Task<IEnumerable<PointTransaction>> GetUserPointHistoryAsync(int userId, int page = 1, int pageSize = 20)
    {
        return await _context.PointTransactions
            .Where(pt => pt.UserId == userId)
            .OrderByDescending(pt => pt.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(pt => pt.RelatedOrder)
            .ToListAsync();
    }

    public async Task<int> GetMaxUsablePointsAsync(int userId, decimal orderAmount)
    {
        var userPoints = await GetUserPointsAsync(userId);
        var maxDiscountAmount = orderAmount * _config.MaxPointsUsagePercentage;
        var maxPointsFromDiscount = (int)Math.Floor(maxDiscountAmount / _config.PointValue);
        
        return Math.Min(userPoints, Math.Max(0, maxPointsFromDiscount));
    }

    public async Task<decimal> CalculatePointsDiscountAsync(int points)
    {
        return points * _config.PointValue;
    }

    public async Task<bool> IsEligibleForWelcomeBonusAsync(int userId)
    {
        // Check if user has any previous orders or point transactions
        var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == userId);
        var hasPointTransactions = await _context.PointTransactions.AnyAsync(pt => pt.UserId == userId && pt.Type == "Bonus");
        
        return !hasOrders && !hasPointTransactions;
    }

    public async Task<bool> AwardWelcomeBonusAsync(int userId)
    {
        try
        {
            if (!await IsEligibleForWelcomeBonusAsync(userId))
                return false;

            var transaction = new PointTransaction
            {
                UserId = userId,
                Points = _config.WelcomeBonusPoints,
                Type = "Bonus",
                Description = "Welcome bonus for new customer",
                CreatedAt = DateTime.Now,
                ExpiresAt = _config.PointsExpirationDays > 0 ? DateTime.Now.AddDays(_config.PointsExpirationDays) : null
            };

            _context.PointTransactions.Add(transaction);

            // Update user's point balance
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Points += _config.WelcomeBonusPoints;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();

            // Send notification about welcome bonus
            await _notificationService.CreateWelcomeBonusNotificationAsync(userId, _config.WelcomeBonusPoints);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidatePointsRedemptionAsync(int userId, int pointsToRedeem)
    {
        if (pointsToRedeem < _config.MinimumRedemptionPoints)
            return false;

        var userPoints = await GetUserPointsAsync(userId);
        return userPoints >= pointsToRedeem;
    }

    public PointsConfig GetPointsConfig()
    {
        return _config;
    }
}
