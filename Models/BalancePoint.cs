using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRN222_Restaurant.Models;

public class BalancePoint
{
    [Key]
    public int Id { get; set; }
    public decimal Point { get; set; }
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; } = null!; // Khởi tạo để không bị null
    
}