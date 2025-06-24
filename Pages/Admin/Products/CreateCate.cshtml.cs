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

        public bool ShowAddModal { get; set; }
        public bool ShowDeleteModal { get; set; }
        public int DeleteCategoryId { get; set; }
        public string DeleteCategoryName { get; set; }

        public async Task<IActionResult> OnGetAsync(bool showAddModal = false, int? deleteId = null, string deleteName = null)
        {
            Categories = (await _menuCategoryService.GetAllAsync()).ToList();
            ShowAddModal = showAddModal;
            ShowDeleteModal = deleteId.HasValue && !string.IsNullOrEmpty(deleteName);
            DeleteCategoryId = deleteId ?? 0;
            DeleteCategoryName = deleteName;

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = (await _menuCategoryService.GetAllAsync()).ToList();
                ShowAddModal = true;
                return Page();
            }

            try
            {
                await _menuCategoryService.AddAsync(Category);
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return Redirect($"/admin/products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm danh mục: " + ex.Message);
                Categories = (await _menuCategoryService.GetAllAsync()).ToList();
                ShowAddModal = true;
                return Page();
            }
        }
        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = (await _menuCategoryService.GetAllAsync()).ToList();
                return Page();
            }

            try
            {
                await _menuCategoryService.UpdateAsync(Category);
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return Redirect($"/admin/products");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật: " + ex.Message);
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