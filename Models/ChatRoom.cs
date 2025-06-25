using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = null!;
        
        public int CustomerId { get; set; }
        
        public int? StaffId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public string Status { get; set; } = "Open"; // Open, InProgress, Closed
        
        // Navigation properties
        public User Customer { get; set; } = null!;
        
        public User? Staff { get; set; }
        
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
