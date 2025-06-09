using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace PRN222_Restaurant.Pages.Admin
{
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardModel : PageModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();

        public void OnGet()
        {
            // In a real application, these would come from your database
            // This is mock data for demonstration purposes
            TotalUsers = 124;
            TotalProducts = 348;
            TotalOrders = 1243;
            TotalRevenue = 34567.89m;

            // Generate mock recent orders
            RecentOrders = new List<Order>
            {
                new Order { Id = 4321, CustomerName = "Nguyễn Văn A", Status = "Completed", Total = 125.50m, Date = DateTime.Now.AddDays(-1) },
                new Order { Id = 4320, CustomerName = "Trần Thị B", Status = "Processing", Total = 75.25m, Date = DateTime.Now.AddDays(-1) },
                new Order { Id = 4319, CustomerName = "Lê Văn C", Status = "Pending", Total = 220.00m, Date = DateTime.Now.AddDays(-2) },
                new Order { Id = 4318, CustomerName = "Phạm Thị D", Status = "Completed", Total = 95.75m, Date = DateTime.Now.AddDays(-2) },
                new Order { Id = 4317, CustomerName = "Hoàng Văn E", Status = "Completed", Total = 150.30m, Date = DateTime.Now.AddDays(-3) }
            };
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
    }
}
