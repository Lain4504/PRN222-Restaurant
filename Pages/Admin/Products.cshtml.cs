using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace PRN222_Restaurant.Pages.Admin
{
    public class ProductsModel : PageModel
    {
        [TempData]
        public string? SuccessMessage { get; set; }

        public List<Product> Products { get; set; } = new List<Product>();
        public int TotalProducts { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (TotalProducts + PageSize - 1) / PageSize;

        public void OnGet(int page = 1)
        {
            CurrentPage = page < 1 ? 1 : page;

            // Mock data - in a real application, this would come from your database
            var allProducts = GetMockProducts();
            TotalProducts = allProducts.Count;

            // Simple pagination logic
            Products = allProducts
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public IActionResult OnPostDelete(int productId)
        {
            // Mock deletion - in a real application, you would delete from your database
            // For demo purposes, we'll just show a success message
            SuccessMessage = $"Sản phẩm ID {productId} đã được xóa thành công";
            return RedirectToPage();
        }

        private List<Product> GetMockProducts()
        {
            // Generate 30 mock products for demonstration
            var categories = new[] { "Đồ uống", "Món chính", "Món tráng miệng", "Món khai vị" };
            var statuses = new[] { "Đang bán", "Hết hàng", "Ngưng bán" };
            var products = new List<Product>();

            var productNames = new[]
            {
                "Cà phê đen", "Cà phê sữa", "Trà đào", "Trà vải", "Trà sữa trân châu",
                "Cơm gà xối mỡ", "Cơm sườn nướng", "Phở bò", "Bún bò Huế", "Mì Quảng",
                "Bánh flan", "Chè thái", "Kem dừa", "Bánh tiramisu", "Chè đậu đỏ",
                "Gỏi cuốn", "Chả giò", "Bánh xèo", "Bánh khọt", "Nem nướng",
                "Nước ép cam", "Nước ép dưa hấu", "Sinh tố bơ", "Sinh tố xoài", "Nước ép dứa",
                "Gà rán", "Pizza", "Hamburger", "Khoai tây chiên", "Salad"
            };

            for (int i = 1; i <= 30; i++)
            {
                var productName = productNames[i % productNames.Length];
                var category = categories[i % categories.Length];
                var price = (i % 10 + 1) * 10000 + 15000; // Random price between 25,000 and 115,000
                
                products.Add(new Product
                {
                    Id = i,
                    Name = productName,
                    Category = category,
                    Price = price,
                    Status = statuses[i % statuses.Length],
                    ImageUrl = $"https://picsum.photos/seed/product{i}/200/200" // Random image
                });
            }

            return products;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
