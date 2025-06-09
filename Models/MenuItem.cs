namespace PRN222_Restaurant.Models;

using System.ComponentModel.DataAnnotations;

public class MenuItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên món là bắt buộc")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    public string Description { get; set; } = null!;

    [Range(1000, double.MaxValue, ErrorMessage = "Giá phải từ 1000 trở lên")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
    public string ImageUrl { get; set; } = null!;

    [Required(ErrorMessage = "Phải chọn danh mục")]
    public int? CategoryId { get; set; } // sửa từ int -> int? để validation hoạt động

    public Category? Category { get; set; }

    public ICollection<OrderItem>? OrderItems { get; set; }
}

