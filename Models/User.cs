namespace PRN222_Restaurant.Models;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = "Customer";

    public bool IsActive { get; set; } = true;

    // Khởi tạo để không bị null
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
 
}

