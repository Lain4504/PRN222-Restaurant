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
        public int TotalProducts { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (TotalProducts + PageSize - 1) / PageSize;
        public int FromRecord => ((CurrentPage - 1) * PageSize) + 1;
        public int ToRecord => Math.Min(CurrentPage * PageSize, TotalProducts);

        public async Task OnGetAsync()
        {
            var pageQuery = HttpContext.Request.Query["page"];
            int.TryParse(pageQuery, out int page);
            CurrentPage = page <= 0 ? 1 : page;

            var query = _context.MenuItems.Include(m => m.Category).AsQueryable();

            TotalProducts = await query.CountAsync();
            Products = await query
                .OrderBy(p => p.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            Statuses = Enum.GetNames(typeof(MenuItemStatus)).ToList();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int productId, int page = 1)
        {
            var product = await _context.MenuItems.FindAsync(productId);
            if (product != null)
            {
                _context.MenuItems.Remove(product);
                await _context.SaveChangesAsync();
                SuccessMessage = $"Sản phẩm ID {productId} đã được xóa thành công";
            }

            // Redirect lại đúng trang hiện tại sau khi xóa
            return Redirect($"/admin/products");
        }
    }

}
