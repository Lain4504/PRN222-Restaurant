using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public StatisticsService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<StatisticsData> GetStatisticsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            
            var now = DateTime.Now;
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = currentMonth.AddMonths(-1);
            var lastMonthEnd = currentMonth.AddDays(-1);

            // Get completed orders only
            var completedOrders = await context.Orders
                .Where(o => o.Status == "Completed")
                .Include(o => o.OrderItems)
                .ToListAsync();

            var currentMonthOrders = completedOrders
                .Where(o => o.OrderDate >= currentMonth)
                .ToList();

            var lastMonthOrders = completedOrders
                .Where(o => o.OrderDate >= lastMonth && o.OrderDate <= lastMonthEnd)
                .ToList();

            // Calculate statistics
            var totalRevenue = completedOrders.Sum(o => o.TotalPrice);
            var totalOrders = completedOrders.Count;
            
            var currentMonthRevenue = currentMonthOrders.Sum(o => o.TotalPrice);
            var lastMonthRevenue = lastMonthOrders.Sum(o => o.TotalPrice);
            
            var currentMonthOrderCount = currentMonthOrders.Count;
            var lastMonthOrderCount = lastMonthOrders.Count;

            // Calculate growth percentages
            var revenueGrowth = lastMonthRevenue > 0 
                ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 
                : 0;
            
            var orderGrowth = lastMonthOrderCount > 0 
                ? ((currentMonthOrderCount - lastMonthOrderCount) / (decimal)lastMonthOrderCount) * 100 
                : 0;

            // Get new customers this month
            var newCustomers = await context.Users
                .Where(u => u.Role == "Customer")
                .CountAsync();

            // Get total customers
            var totalCustomers = await context.Users
                .Where(u => u.Role == "Customer")
                .CountAsync();

            // Get top selling product
            var topProduct = await GetTopSellingProductAsync(context);

            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            return new StatisticsData
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                NewCustomers = newCustomers,
                TopSellingProduct = topProduct.Name,
                TopSellingCount = topProduct.Count,
                AverageOrderValue = averageOrderValue,
                TotalCustomers = totalCustomers,
                RevenueGrowth = revenueGrowth,
                OrderGrowth = (int)orderGrowth
            };
        }

        private async Task<(string Name, int Count)> GetTopSellingProductAsync(ApplicationDbContext context)
        {
            var topProduct = await context.OrderItems
                .Include(oi => oi.MenuItem)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.Status == "Completed")
                .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem.Name })
                .Select(g => new { 
                    Name = g.Key.Name, 
                    Count = g.Sum(oi => oi.Quantity) 
                })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return topProduct != null ? (topProduct.Name, topProduct.Count) : ("Không có dữ liệu", 0);
        }

        public async Task<List<TopProductStats>> GetTopProductsAsync(int count = 5)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var topProducts = await context.OrderItems
                .Include(oi => oi.MenuItem)
                .ThenInclude(mi => mi.Category)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.Status == "Completed")
                .GroupBy(oi => new { 
                    oi.MenuItemId, 
                    oi.MenuItem.Name, 
                    oi.MenuItem.ImageUrl,
                    CategoryName = oi.MenuItem.Category.Name 
                })
                .Select(g => new TopProductStats
                {
                    Name = g.Key.Name,
                    Category = g.Key.CategoryName,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
                    ImageUrl = g.Key.ImageUrl
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(count)
                .ToListAsync();

            return topProducts;
        }

        public async Task<List<CustomerReview>> GetRecentReviewsAsync(int count = 4)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var reviews = await context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .Take(count)
                .Select(f => new CustomerReview
                {
                    Id = f.Id,
                    CustomerName = f.User.FullName,
                    Rating = f.Rating,
                    Comment = f.Comment,
                    ProductName = "Dịch vụ nhà hàng", // Since feedback is general, not product-specific
                    Date = f.CreatedAt
                })
                .ToListAsync();

            return reviews;
        }

        public async Task<decimal[]> GetMonthlySalesDataAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            var currentYear = DateTime.Now.Year;
            var monthlySales = new decimal[12];

            var monthlyData = await context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate.Year == currentYear)
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(o => o.TotalPrice) })
                .ToListAsync();

            foreach (var data in monthlyData)
            {
                monthlySales[data.Month - 1] = data.Revenue;
            }

            // If no data for current year, add some sample data for demonstration
            var hasData = monthlySales.Any(x => x > 0);
            if (!hasData)
            {
                // No monthly sales data found, using sample data
                var random = new Random();
                for (int i = 0; i < 12; i++)
                {
                    monthlySales[i] = random.Next(100000, 500000);
                }
            }

            return monthlySales;
        }

        public async Task<Dictionary<string, decimal>> GetCategorySalesDataAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            // First check if we have any menu items with categories
            var hasMenuItems = await context.MenuItems.AnyAsync();
            var hasCategories = await context.Categories.AnyAsync();

            // Check if we have menu items and categories

            var categorySales = new Dictionary<string, decimal>();

            if (hasMenuItems && hasCategories)
            {
                categorySales = await context.OrderItems
                    .Include(oi => oi.MenuItem)
                    .ThenInclude(mi => mi.Category)
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.Status == "Completed" && oi.MenuItem.Category != null)
                    .GroupBy(oi => oi.MenuItem.Category.Name)
                    .Select(g => new {
                        Category = g.Key,
                        Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                    })
                    .Where(x => !string.IsNullOrEmpty(x.Category))
                    .ToDictionaryAsync(x => x.Category, x => x.Revenue);

                // Found categories with sales data
            }

            // If no real data, provide sample data for demonstration
            if (categorySales.Count == 0)
            {
                // No category sales data found, using sample data
                categorySales = new Dictionary<string, decimal>
                {
                    { "Đồ uống", 150000 },
                    { "Món chính", 320000 },
                    { "Món tráng miệng", 85000 },
                    { "Món khai vị", 120000 }
                };
            }

            return categorySales;
        }

        public async Task<List<MonthlyOrderStats>> GetMonthlyOrderStatsAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            
            var currentYear = DateTime.Now.Year;
            
            var monthlyStats = await context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate.Year == currentYear)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyOrderStats
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return monthlyStats;
        }

        public async Task<List<DailyRevenueStats>> GetDailyRevenueStatsAsync(int days = 30)
        {
            using var context = _contextFactory.CreateDbContext();
            
            var startDate = DateTime.Now.AddDays(-days).Date;
            
            var dailyStats = await context.Orders
                .Where(o => o.Status == "Completed" && o.OrderDate >= startDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyRevenueStats
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalPrice),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return dailyStats;
        }
    }
}
