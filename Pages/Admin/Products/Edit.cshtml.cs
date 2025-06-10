using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;


namespace PRN222_Restaurant.Pages.Admin.Products
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MenuItem Product { get; set; } = new MenuItem();
        public List<Category> Categories { get; set; } = new();
        public List<string> Statuses { get; set; } = new();

        public bool IsNewProduct => Product.Id == 0;

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Categories = await _context.Categories.ToListAsync();
            Statuses = Enum.GetNames(typeof(MenuItemStatus)).ToList();

            if (id.HasValue)
            {
                var product = await _context.MenuItems
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id.Value);

                if (product == null)
                    return NotFound();

                Product = product;
            }
            else
            {
                Product = new MenuItem
                {
                    Status = MenuItemStatus.Available,
                    ImageUrl = ""
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _context.Categories.ToListAsync();
            Statuses = Enum.GetNames(typeof(MenuItemStatus)).ToList();

            if (!ModelState.IsValid)
                return Page();

            if (IsNewProduct)
            {
                _context.MenuItems.Add(Product);
                SuccessMessage = "Đã thêm sản phẩm mới thành công";
            }
            else
            {
                var existingProduct = await _context.MenuItems.FindAsync(Product.Id);
                if (existingProduct == null)
                    return NotFound();

                existingProduct.Name = Product.Name;
                existingProduct.Description = Product.Description;
                existingProduct.Price = Product.Price;
                existingProduct.Status = Product.Status;
                existingProduct.CategoryId = Product.CategoryId;
                existingProduct.ImageUrl = Product.ImageUrl;

                SuccessMessage = $"Đã cập nhật sản phẩm {Product.Name} thành công";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/admin/products");
        }

    }
}
