namespace PRN222_Restaurant.Models;

public class PointsConfig
{
    public static string ConfigName => "Points";
    
    /// <summary>
    /// Points earned per dollar spent (e.g., 0.05 = 5% of order value)
    /// </summary>
    public decimal PointsPerDollar { get; set; } = 0.05m; // 5% of order value

    /// <summary>
    /// Minimum order amount to earn points
    /// </summary>
    public decimal MinimumOrderAmount { get; set; } = 10;

    /// <summary>
    /// Value of each point in dollars (e.g., 1 point = $1)
    /// </summary>
    public decimal PointValue { get; set; } = 1.0m; // 1 point = $1

    /// <summary>
    /// Minimum points required for redemption
    /// </summary>
    public int MinimumRedemptionPoints { get; set; } = 1; // Can use any amount

    /// <summary>
    /// Maximum percentage of order that can be paid with points
    /// </summary>
    public decimal MaxPointsUsagePercentage { get; set; } = 1.0m; // 100% - can pay entire order

    /// <summary>
    /// Points expiration in days (0 = never expire)
    /// </summary>
    public int PointsExpirationDays { get; set; } = 0; // Never expire

    /// <summary>
    /// Bonus points for first order (worth $5)
    /// </summary>
    public int WelcomeBonusPoints { get; set; } = 5; // $5 welcome bonus
}
