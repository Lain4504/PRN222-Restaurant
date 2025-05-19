namespace PRN222_Restaurant.Models;

public class Promotion
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}