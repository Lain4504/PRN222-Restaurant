using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PRN222_Restaurant.Pages.Admin
{
    public class StatsModel : PageModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public string TopSellingProduct { get; set; } = string.Empty;
        public int TopSellingCount { get; set; }
        
        public List<TopProductStats> TopProducts { get; set; } = new List<TopProductStats>();
        public List<CustomerReview> RecentReviews { get; set; } = new List<CustomerReview>();
        
        public decimal[] MonthlySalesData { get; set; } = new decimal[12];
        public Dictionary<string, decimal> CategorySalesData { get; set; } = new Dictionary<string, decimal>();

        public void OnGet()
        {
            // Generate mock statistics data
            GenerateMockStats();
        }

        private void GenerateMockStats()
        {
            // General statistics
            TotalRevenue = 248750000; // 248,750,000 VND
            TotalOrders = 1243;
            NewCustomers = 87;
            TopSellingProduct = "Cà phê sữa";
            TopSellingCount = 356;
            
            // Monthly sales data (12 months)
            Random random = new Random();
            for (int i = 0; i < 12; i++)
            {
                // Generate random monthly revenue between 15M and 25M
                MonthlySalesData[i] = random.Next(15000000, 25000000);
            }
            
            // Ensure current month (may) has the highest revenue for demo
            MonthlySalesData[4] = 24500000; // May
            
            // Category sales data
            CategorySalesData = new Dictionary<string, decimal>
            {
                { "Đồ uống", 98500000 },
                { "Món chính", 76250000 },
                { "Món tráng miệng", 42000000 },
                { "Món khai vị", 32000000 }
            };
            
            // Top selling products
            TopProducts = new List<TopProductStats>
            {
                new TopProductStats
                {
                    Name = "Cà phê sữa",
                    Category = "Đồ uống",
                    QuantitySold = 356,
                    Revenue = 10680000,
                    ImageUrl = "https://picsum.photos/seed/product1/200/200"
                },
                new TopProductStats
                {
                    Name = "Trà sữa trân châu",
                    Category = "Đồ uống",
                    QuantitySold = 287,
                    Revenue = 10045000,
                    ImageUrl = "https://picsum.photos/seed/product5/200/200"
                },
                new TopProductStats
                {
                    Name = "Cơm gà xối mỡ",
                    Category = "Món chính",
                    QuantitySold = 264,
                    Revenue = 13200000,
                    ImageUrl = "https://picsum.photos/seed/product6/200/200"
                },
                new TopProductStats
                {
                    Name = "Phở bò",
                    Category = "Món chính",
                    QuantitySold = 245,
                    Revenue = 12250000,
                    ImageUrl = "https://picsum.photos/seed/product8/200/200"
                },
                new TopProductStats
                {
                    Name = "Bánh flan",
                    Category = "Món tráng miệng",
                    QuantitySold = 198,
                    Revenue = 5940000,
                    ImageUrl = "https://picsum.photos/seed/product11/200/200"
                }
            };
            
            // Recent reviews
            RecentReviews = new List<CustomerReview>
            {
                new CustomerReview
                {
                    Id = 1,
                    CustomerName = "Nguyễn Văn A",
                    Rating = 5,
                    Comment = "Món ăn rất ngon, phục vụ nhanh. Tôi sẽ quay lại!",
                    ProductName = "Cơm gà xối mỡ",
                    Date = DateTime.Now.AddDays(-1)
                },
                new CustomerReview
                {
                    Id = 2,
                    CustomerName = "Trần Thị B",
                    Rating = 4,
                    Comment = "Nước uống ngon, giá hợp lý. Không gian hơi ồn.",
                    ProductName = "Trà sữa trân châu",
                    Date = DateTime.Now.AddDays(-2)
                },
                new CustomerReview
                {
                    Id = 3,
                    CustomerName = "Lê Văn C",
                    Rating = 3,
                    Comment = "Phục vụ hơi chậm nhưng món ăn ngon.",
                    ProductName = "Phở bò",
                    Date = DateTime.Now.AddDays(-3)
                },
                new CustomerReview
                {
                    Id = 4,
                    CustomerName = "Phạm Thị D",
                    Rating = 5,
                    Comment = "Quán đẹp, món ngon, nhân viên thân thiện!",
                    ProductName = "Bánh flan",
                    Date = DateTime.Now.AddDays(-5)
                }
            };
        }
    }

    public class TopProductStats
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CustomerReview
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
