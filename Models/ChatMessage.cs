using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        
        public int ChatRoomId { get; set; }
        
        public int SenderId { get; set; }
        
        [Required]
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Content { get; set; } = null!;
        
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        public string MessageType { get; set; } = "Text"; // Text, Image, File, System
        
        public string? AttachmentUrl { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        // Navigation properties
        public ChatRoom ChatRoom { get; set; } = null!;
        
        public User Sender { get; set; } = null!;
    }
}
