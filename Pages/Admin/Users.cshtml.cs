using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Helper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;
        private const int DefaultPageSize = 10;

        public UsersModel(IUserService userService)
        {
            _userService = userService;
        }

        public PagedResult<User> UsersResult { get; set; } = new PagedResult<User>();
        public List<User> Users => UsersResult.Items;

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DefaultPageSize;

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if current user is Admin
            if (!AuthHelper.IsAdmin(HttpContext.User))
            {
                return Redirect("/admin/login");
            }

            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = DefaultPageSize;

            UsersResult = await _userService.GetPagedUsersAsync(CurrentPage, PageSize);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int userId)
        {
            // Check if current user is Admin
            if (!AuthHelper.IsAdmin(HttpContext.User))
            {
                return Redirect("/admin/login");
            }

            if (userId <= 0)
                return BadRequest();

            bool deleted = await _userService.DeleteUserAsync(userId);

            if (deleted)
            {
                SuccessMessage = "Xóa người dùng thành công!";
            }
            else
            {
                SuccessMessage = "Xóa người dùng thất bại!";
            }

            return RedirectToPage(new { currentPage = CurrentPage, pageSize = PageSize });
        }
    }
}