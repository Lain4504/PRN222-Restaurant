namespace PRN222_Restaurant.Models
{
    public class UserConnection
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public string ConnectionId { get; set; } = null!;
        
        public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        public bool IsOnline { get; set; } = true;
        
        public string? CurrentPage { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
}
