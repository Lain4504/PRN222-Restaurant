using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class MenuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MenuModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MenuItem> MenuItems { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<MenuItem> TopRecommendations { get; set; } = new();

        public Dictionary<string, string> MenuStatusDisplayNames { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public const int PageSize = 9;

        public async Task OnGet()
        {
            Categories = await _context.Categories.ToListAsync();

            var query = _context.MenuItems.Include(m => m.Category).AsQueryable();

            if (CategoryId.HasValue)
                query = query.Where(m => m.CategoryId == CategoryId);

            if (!string.IsNullOrEmpty(Status))
                query = query.Where(m => m.Status.ToString().ToLower() == Status.ToLower());

            if (!string.IsNullOrWhiteSpace(SearchTerm))
                query = query.Where(m => m.Name.Contains(SearchTerm) || m.Description.Contains(SearchTerm));

            if (MinPrice.HasValue)
                query = query.Where(m => m.Price >= MinPrice);

            if (MaxPrice.HasValue)
                query = query.Where(m => m.Price <= MaxPrice);

            int totalItems = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling((double)totalItems / PageSize);

            MenuItems = await query
                .OrderBy(m => m.Name)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            TopRecommendations = await _context.MenuItems
                .OrderByDescending(m => m.BuyCount)
                .ThenByDescending(m => m.Price)
                .ThenBy(m => m.Status)
                .Take(3)
                .ToListAsync();

            MenuStatusDisplayNames = new()
            {
                { "available", "Còn món" },
                { "unavailable", "Hết món" }
            };
        }
    }
}
