using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services.IService;

public interface IPointsService
{
    /// <summary>
    /// Calculate points earned from an order amount
    /// </summary>
    Task<int> CalculatePointsEarnedAsync(decimal orderAmount);
    
    /// <summary>
    /// Award points to a user for a successful payment
    /// </summary>
    Task<bool> AwardPointsAsync(int userId, int orderId, decimal orderAmount, string description = "Order payment");
    
    /// <summary>
    /// Redeem points for a discount on an order
    /// </summary>
    Task<decimal> RedeemPointsAsync(int userId, int pointsToRedeem, int orderId, string description = "Points redemption");
    
    /// <summary>
    /// Get user's current point balance
    /// </summary>
    Task<int> GetUserPointsAsync(int userId);
    
    /// <summary>
    /// Get user's point transaction history
    /// </summary>
    Task<IEnumerable<PointTransaction>> GetUserPointHistoryAsync(int userId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Calculate maximum points that can be used for an order
    /// </summary>
    Task<int> GetMaxUsablePointsAsync(int userId, decimal orderAmount);
    
    /// <summary>
    /// Calculate discount amount from points
    /// </summary>
    Task<decimal> CalculatePointsDiscountAsync(int points);
    
    /// <summary>
    /// Check if user is eligible for welcome bonus
    /// </summary>
    Task<bool> IsEligibleForWelcomeBonusAsync(int userId);
    
    /// <summary>
    /// Award welcome bonus points to new user
    /// </summary>
    Task<bool> AwardWelcomeBonusAsync(int userId);
    
    /// <summary>
    /// Validate if user has enough points for redemption
    /// </summary>
    Task<bool> ValidatePointsRedemptionAsync(int userId, int pointsToRedeem);
    
    /// <summary>
    /// Get points configuration
    /// </summary>
    PointsConfig GetPointsConfig();
}
