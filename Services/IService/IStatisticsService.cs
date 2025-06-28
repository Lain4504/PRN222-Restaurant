using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services.IService
{
    public interface IStatisticsService
    {
        Task<StatisticsData> GetStatisticsAsync();
        Task<List<TopProductStats>> GetTopProductsAsync(int count = 5);
        Task<List<CustomerReview>> GetRecentReviewsAsync(int count = 4);
        Task<decimal[]> GetMonthlySalesDataAsync();
        Task<Dictionary<string, decimal>> GetCategorySalesDataAsync();
        Task<List<MonthlyOrderStats>> GetMonthlyOrderStatsAsync();
        Task<List<DailyRevenueStats>> GetDailyRevenueStatsAsync(int days = 30);
    }

    public class StatisticsData
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public string TopSellingProduct { get; set; } = string.Empty;
        public int TopSellingCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalCustomers { get; set; }
        public decimal RevenueGrowth { get; set; }
        public int OrderGrowth { get; set; }
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

    public class MonthlyOrderStats
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DailyRevenueStats
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
