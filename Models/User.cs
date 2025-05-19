namespace PRN222_Restaurant.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "Customer"; // Admin, Staff, Customer
    public bool IsActive { get; set; } = true;

    public ICollection<Reservation> Reservations { get; set; }
    public ICollection<Order> Orders { get; set; }
}

