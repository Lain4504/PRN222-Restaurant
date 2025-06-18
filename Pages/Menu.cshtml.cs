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

        public async Task OnGet()
        {
            // Lấy danh sách danh mục và món ăn từ cơ sở dữ liệu
            Categories = await _context.Categories.ToListAsync();
            MenuItems = await _context.MenuItems
                .Include(m => m.Category) // Bao gồm thông tin danh mục
                .ToListAsync();
        }
    }
}
