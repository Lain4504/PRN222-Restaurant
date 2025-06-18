using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Data;
using Microsoft.EntityFrameworkCore;

namespace PRN222_Restaurant.Pages
{
    public class MenuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MenuModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<MenuItem> TopRecommendations { get; set; } = new List<MenuItem>();


        public async Task OnGet()
        {
            // Lấy danh sách danh mục và món ăn từ cơ sở dữ liệu
            Categories = await _context.Categories.ToListAsync();

            // Lấy danh sách món ăn chỉ với Status = Available
            MenuItems = await _context.MenuItems
                .Where(m => m.Status == MenuItemStatus.Available)
                .Include(m => m.Category)
                .ToListAsync();

            // Lấy 3 món bán chạy nhất (BuyCount cao hơn, Tiền cao hơn và Status còn hàng hay không)
            TopRecommendations = await _context.MenuItems
                .OrderByDescending(m => m.BuyCount)
                .ThenByDescending(m => m.Price)
                .ThenBy(m => m.Status)
                .Take(3)
                .ToListAsync();
        }
    }
}
