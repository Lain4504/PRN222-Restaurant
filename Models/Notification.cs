namespace PRN222_Restaurant.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public string? RelatedUrl { get; set; } // URL liên quan (ví dụ: link đến order)
    public int? RelatedId { get; set; } // ID liên quan (ví dụ: OrderId)

    public User User { get; set; } = null!;
}