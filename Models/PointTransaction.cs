using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Models;

public class PointTransaction
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int Points { get; set; } // Positive for earning, negative for spending
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = null!; // "Earned", "Redeemed", "Expired", "Bonus"
    
    [StringLength(200)]
    public string Description { get; set; } = null!;
    
    public int? RelatedOrderId { get; set; } // Order that generated/used these points
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime? ExpiresAt { get; set; } // Points expiration date (optional)
    
    // Navigation properties
    public User User { get; set; } = null!;
    
    public Order? RelatedOrder { get; set; }
}
