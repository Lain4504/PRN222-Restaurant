﻿namespace PRN222_Restaurant.Models;

using System.ComponentModel.DataAnnotations;

public class MenuItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên món là bắt buộc")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    public string Description { get; set; } = null!;

    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
    public string ImageUrl { get; set; } = null!;

    [Required(ErrorMessage = "Phải chọn danh mục")]
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<OrderItem>? OrderItems { get; set; }

    [Required(ErrorMessage = "Tình trạng món ăn là bắt buộc")]
    public MenuItemStatus Status { get; set; }

    public int BuyCount { get; set; } = 0;
}

public enum MenuItemStatus
{
    Available,   // Còn món
    Unavailable  // Hết món
}

