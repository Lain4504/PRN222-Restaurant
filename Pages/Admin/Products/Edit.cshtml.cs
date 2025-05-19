using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace PRN222_Restaurant.Pages.Admin.Products
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public Product Product { get; set; } = new Product();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public bool IsNewProduct => Product.Id == 0;

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (id.HasValue)
            {
                // Edit existing product
                var product = GetProductById(id.Value);
                if (product == null)
                {
                    return NotFound();
                }
                
                Product = product;
            }
            else
            {
                // Create new product
                Product = new Product
                {
                    Status = "Đang bán", // Default status for new products
                    ImageUrl = "" // No image by default
                };
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            // In a real application, you would save the image file to a storage location
            // and update the ImageUrl property with the file path or URL
            
            // For demo purposes, we'll just use a placeholder image if a new file is uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // In a real application, save the file and get the URL
                // For demo, we'll use a placeholder
                Product.ImageUrl = $"https://picsum.photos/seed/product{DateTime.Now.Ticks}/200/200";
            }
            
            // In a real application, you would save to database
            // For demo purposes, we'll just redirect with a success message
            
            if (IsNewProduct)
            {
                SuccessMessage = "Đã thêm sản phẩm mới thành công";
            }
            else
            {
                SuccessMessage = $"Đã cập nhật sản phẩm {Product.Name} thành công";
            }

            return RedirectToPage("/Admin/Products");
        }

        private Product? GetProductById(int id)
        {
            // In a real application, you would get this from your database
            // For demo purposes, we'll just return a mock product
            if (id <= 0 || id > 30) return null;

            var categories = new[] { "Đồ uống", "Món chính", "Món tráng miệng", "Món khai vị" };
            var statuses = new[] { "Đang bán", "Hết hàng", "Ngưng bán" };
            
            var productNames = new[]
            {
                "Cà phê đen", "Cà phê sữa", "Trà đào", "Trà vải", "Trà sữa trân châu",
                "Cơm gà xối mỡ", "Cơm sườn nướng", "Phở bò", "Bún bò Huế", "Mì Quảng",
                "Bánh flan", "Chè thái", "Kem dừa", "Bánh tiramisu", "Chè đậu đỏ",
                "Gỏi cuốn", "Chả giò", "Bánh xèo", "Bánh khọt", "Nem nướng",
                "Nước ép cam", "Nước ép dưa hấu", "Sinh tố bơ", "Sinh tố xoài", "Nước ép dứa",
                "Gà rán", "Pizza", "Hamburger", "Khoai tây chiên", "Salad"
            };

            var productName = productNames[id % productNames.Length];
            var category = categories[id % categories.Length];
            var price = (id % 10 + 1) * 10000 + 15000; // Random price between 25,000 and 115,000
            
            return new Product
            {
                Id = id,
                Name = productName,
                Category = category,
                Price = price,
                Status = statuses[id % statuses.Length],
                ImageUrl = $"https://picsum.photos/seed/product{id}/200/200", // Random image
                Description = $"Đây là mô tả chi tiết về sản phẩm {productName}. Sản phẩm thuộc danh mục {category} với chất lượng cao và giá cả phải chăng. Phù hợp cho mọi khách hàng."
            };
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
        public string Description { get; set; } = string.Empty;
    }
}
