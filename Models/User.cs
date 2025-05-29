using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    // Không đánh dấu [Required] vì bạn không nhập mật khẩu từ form
    public string? PasswordHash { get; set; }

    public string Role { get; set; } = "Customer";

    public bool IsActive { get; set; } = true;

    // Khởi tạo để không bị null
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public BalancePoint? BalancePoint { get; set; } 
}

