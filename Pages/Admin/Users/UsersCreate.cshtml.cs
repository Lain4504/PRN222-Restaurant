using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class UsersCreateModel : PageModel
    {
        private readonly IUserService _userService;

        public UsersCreateModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        public void OnGet()
        {
            // Logic khi load trang nếu cần
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine("Validation error: " + error);
                }

                return Page();
            }

            await _userService.CreateUserAsync(User);

            var addedUser = await _userService.GetUserByEmailAsync(User.Email);
            if (addedUser != null)
            {
                TempData["SuccessMessage"] = "Thêm người dùng thành công!";
                return RedirectToPage("/Admin/Users");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Thêm người dùng thất bại, vui lòng thử lại.");
                return Page();
            }
        }

    }
}
