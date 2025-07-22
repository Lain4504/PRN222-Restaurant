namespace PRN222_Restaurant.Models;

public class PointsConfig
{
    public static string ConfigName => "Points";
    
    /// <summary>
    /// Points earned per VND spent (e.g., 0.00001 = 1 point per 100,000 VND)
    /// </summary>
    public decimal PointsPerVND { get; set; } = 0.00001m; // 1 point per 100,000 VND spent

    /// <summary>
    /// Minimum order amount to earn points (in VND)
    /// </summary>
    public decimal MinimumOrderAmount { get; set; } = 100000; // 100,000 VND

    /// <summary>
    /// Value of each point in VND (e.g., 1 point = 5,000 VND)
    /// </summary>
    public decimal PointValue { get; set; } = 5000m; // 1 point = 5,000 VND

    /// <summary>
    /// Minimum points required for redemption
    /// </summary>
    public int MinimumRedemptionPoints { get; set; } = 1; // Can use any amount

    /// <summary>
    /// Maximum percentage of order that can be paid with points
    /// </summary>
    public decimal MaxPointsUsagePercentage { get; set; } = 0.3m; // 30% - maximum 30% discount

    /// <summary>
    /// Points expiration in days (0 = never expire)
    /// </summary>
    public int PointsExpirationDays { get; set; } = 0; // Never expire

    /// <summary>
    /// Bonus points for first order (worth 50,000 VND)
    /// </summary>
    public int WelcomeBonusPoints { get; set; } = 5; // 5 points = 50,000 VND welcome bonus
}
