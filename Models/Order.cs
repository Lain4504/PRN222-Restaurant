﻿namespace PRN222_Restaurant.Models;

public class Order
{
    public int Id { get; set; }
    public int? UserId { get; set; }  // Có thể là order của khách vãng lai
    public int? TableId { get; set; } // Cho dine-in
    public DateTime OrderDate { get; set; }
    public DateTime? ReservationTime { get; set; }  // Thời gian đặt bàn cho pre-order
    public string OrderType { get; set; } = "Immediate"; // "Immediate" hoặc "PreOrder"
    public string Status { get; set; } = "Pending"; // Pending, Preparing, Served, Completed, Cancelled, Paid, Paid Deposit
    public decimal TotalPrice { get; set; }
    public int? PointsUsed { get; set; }  // Số points đã sử dụng cho đơn hàng này
    public string? Note { get; set; }
    public int? NumberOfGuests { get; set; }  // Số lượng khách cho pre-order

    public User? User { get; set; }
    public Table? Table { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
