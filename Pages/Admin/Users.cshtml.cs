using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;

        public UsersModel(IUserService userService)
        {
            _userService = userService;
        }

        // Dữ liệu sẽ được bind vào view
        public IList<User> Users { get; set; } = new List<User>();

        // Tổng số user (dùng để hiển thị phân trang)
        public int TotalUsers { get; set; }

        // Tổng số trang (phân trang)
        public int TotalPages { get; set; }

        // Trang hiện tại
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        // Số user mỗi trang
        private const int PageSize = 10;

        // Dùng để hiển thị thông báo thành công (nếu có)
        [TempData]
        public string SuccessMessage { get; set; }

        // Xử lý GET: load danh sách user theo phân trang
        public async Task<IActionResult> OnGetAsync()
        {
            // Lấy tất cả user
            var allUsers = await _userService.GetAllUsersAsync();

            TotalUsers = allUsers.Count();

            // Tính tổng số trang
            TotalPages = (TotalUsers + PageSize - 1) / PageSize;

            // Giới hạn current page trong khoảng hợp lệ
            if (CurrentPage < 1)
                CurrentPage = 1;
            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            // Lấy danh sách user theo phân trang
            Users = allUsers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        // Xử lý POST khi xóa user
        public async Task<IActionResult> OnPostDeleteAsync(int userId)
        {
            if (userId <= 0)
                return BadRequest();

            bool deleted = await _userService.DeleteUserAsync(userId);

            if (deleted)
            {
                SuccessMessage = "Xóa người dùng thành công!";
            }
            else
            {
                // Xử lý trường hợp xóa thất bại nếu cần
                SuccessMessage = "Xóa người dùng thất bại!";
            }

            return RedirectToPage(); // Reload lại trang
        }
    }
}
