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
        public SelectList? StatusOptions { get; set; }

        private async Task LoadCategoryOptionsAsync()
        {
            var categories = await _menuCategoryService.GetAllAsync();
            CategoryOptions = new SelectList(categories, "Id", "Name");
        }

        private void LoadStatusOptions()
        {
            StatusOptions = new SelectList(
                Enum.GetValues(typeof(MenuItemStatus))
                    .Cast<MenuItemStatus>()
                    .Select(s => new { Value = s, Text = s.ToString() }),
                "Value", "Text"
            );
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoryOptionsAsync();
            LoadStatusOptions();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoryOptionsAsync();
                LoadStatusOptions();
                return Page();
            }

            try
            {
                await _menuItemService.AddAsync(MenuItem);
                TempData["SuccessMessage"] = "Thêm món thành công!";
                return RedirectToPage("/admin/products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi thêm món: " + ex.Message);
                await LoadCategoryOptionsAsync();
                LoadStatusOptions();
                return Page();
            }
        }
    }
}
