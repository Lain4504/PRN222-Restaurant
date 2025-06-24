using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Pages.Admin
{
    public class ProductsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [TempData]
        public string? SuccessMessage { get; set; }

        public List<MenuItem> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<string> Statuses { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedCategory { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedStatus { get; set; }

        [BindProperty(SupportsGet = true, Name = "pageNumber")]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;
        public int TotalProducts { get; set; }
        public int TotalPages => (TotalProducts + PageSize - 1) / PageSize;
        public int FromRecord => ((CurrentPage - 1) * PageSize) + 1;
        public int ToRecord => Math.Min(CurrentPage * PageSize, TotalProducts);

        public async Task OnGetAsync()
        {
            var query = _context.MenuItems.Include(m => m.Category).AsQueryable();

            // Áp dụng filter nếu có
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(SelectedCategory))
            {
                query = query.Where(p => p.Category.Name == SelectedCategory);
            }

            if (!string.IsNullOrEmpty(SelectedStatus))
            {
                if (Enum.TryParse<MenuItemStatus>(SelectedStatus, out var status))
                {
                    query = query.Where(p => p.Status == status);
                }
            }

            TotalProducts = await query.CountAsync();
            Products = await query
                .OrderBy(p => p.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Statuses = Enum.GetNames(typeof(MenuItemStatus)).ToList();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int productId)
        {
            var product = await _context.MenuItems.FindAsync(productId);
            if (product != null)
            {
                _context.MenuItems.Remove(product);
                await _context.SaveChangesAsync();
                SuccessMessage = $"Sản phẩm ID {productId} đã được xóa thành công.";
            }

            return RedirectToPage("/Admin/Products", new
            {
                CurrentPage,
                SearchTerm,
                SelectedCategory,
                SelectedStatus
            });
        }
    }
}
