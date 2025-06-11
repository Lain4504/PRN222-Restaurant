using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Pages.Admin.Products
{
    public class EditCateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditCateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; } = new Category();

        public bool IsNewCategory => Category.Id == 0;

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == id.Value);

                if (category == null)
                    return NotFound();

                Category = category;
            }
            else
            {
                Category = new Category();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Vui lòng kiểm tra lại thông tin nhập vào.";
                return Page();
            }

            if (IsNewCategory)
            {
                _context.Categories.Add(Category);
                SuccessMessage = "Đã thêm danh mục mới thành công";
            }
            else
            {
                var existingCategory = await _context.Categories.FindAsync(Category.Id);
                if (existingCategory == null)
                    return NotFound();

                existingCategory.Name = Category.Name;
                SuccessMessage = $"Đã cập nhật danh mục {Category.Name} thành công";
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("/admin/products");
            }
            catch (DbUpdateException ex)
            {
                ErrorMessage = "Lỗi khi lưu danh mục. Vui lòng thử lại.";
                return Page();
            }
        }
    }
}