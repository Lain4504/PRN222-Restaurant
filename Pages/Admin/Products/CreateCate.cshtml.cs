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
        public Category Category { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Categories = (await _menuCategoryService.GetAllAsync()).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = (await _menuCategoryService.GetAllAsync()).ToList();
                return Page();
            }

            try
            {
                await _menuCategoryService.AddAsync(Category);
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return Redirect($"/admin/products"); // Refresh lại trang để hiển thị danh sách mới
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm danh mục: " + ex.Message);
                Categories = (await _menuCategoryService.GetAllAsync()).ToList();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _menuCategoryService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi xóa: " + ex.Message);
            }

            return Redirect($"/admin/products");
        }
    }
}
