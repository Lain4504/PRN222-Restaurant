using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Pages.Admin.Products
{
    public class CreateCateModel : PageModel
    {
        private readonly IMenuCategoryService _menuCategoryService;

        public CreateCateModel(IMenuCategoryService menuCategoryService)
        {
            _menuCategoryService = menuCategoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = new Category();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _menuCategoryService.AddAsync(Category);
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToPage("/admin/products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm danh mục: " + ex.Message);
                return Page();
            }
        }
    }
}
