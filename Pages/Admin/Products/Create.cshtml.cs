using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Pages.Admin.Products
{
    public class CreateModel : PageModel
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IMenuCategoryService _menuCategoryService;

        public CreateModel(IMenuItemService menuItemService, IMenuCategoryService menuCategoryService)
        {
            _menuItemService = menuItemService;
            _menuCategoryService = menuCategoryService;
        }

        [BindProperty]
        public MenuItem MenuItem { get; set; } = new MenuItem();

        public SelectList? CategoryOptions { get; set; }

        private async Task LoadCategoryOptionsAsync()
        {
            var categories = await _menuCategoryService.GetAllAsync();
            CategoryOptions = new SelectList(categories, "Id", "Name");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoryOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoryOptionsAsync();
                return Page();
            }

            try
            {
                // Sử dụng dữ liệu từ form thay vì hardcode
                await _menuItemService.AddAsync(MenuItem);
                TempData["SuccessMessage"] = "Thêm món thành công!";
                return RedirectToPage("/admin/products");
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm món: " + ex.Message);
                await LoadCategoryOptionsAsync();
                return Page();
            }
        }
    }
}