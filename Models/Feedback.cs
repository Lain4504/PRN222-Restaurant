using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Models;

public class Feedback
{
    public int Id { get; set; }

    [Required(ErrorMessage = "UserId là bắt buộc.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Nội dung phản hồi không được để trống.")]
    [StringLength(500, ErrorMessage = "Nội dung phản hồi tối đa 500 ký tự.")]
    public string Comment { get; set; } = null!;

    [Range(1, 5, ErrorMessage = "Đánh giá phải nằm trong khoảng từ 1 đến 5.")]
    public int Rating { get; set; } // 1 - 5

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public User User { get; set; } = null!;
}
