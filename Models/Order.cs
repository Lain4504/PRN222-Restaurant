namespace PRN222_Restaurant.Models;

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }  // Có thể là order của khách vãng lai
    public int? TableId { get; set; } // Cho dine-in
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Preparing, Served, Completed, Cancelled
    public decimal TotalPrice { get; set; }

    public User? User { get; set; }
    public Table? Table { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}
