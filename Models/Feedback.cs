namespace PRN222_Restaurant.Models;

public class Feedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Comment { get; set; }
    public int Rating { get; set; } // 1 - 5
    public DateTime CreatedAt { get; set; }

    public User User { get; set; }
}
