using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using System.Threading.Tasks;
using BCrypt.Net;


namespace PRN222_Restaurant.Pages.Admin.Users
{
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;

        public EditModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public User User { get; set; } = new User();

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        public bool IsNewUser => User.Id == 0;

        public string[] Roles { get; set; } = new[] { "Admin", "Manager", "Staff", "User" };

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue)
            {
                var user = await _userService.GetUserByIdAsync(id.Value);
                if (user == null)
                {
                    return NotFound();
                }

                User = user;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!string.IsNullOrEmpty(Password) || IsNewUser)
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.";
                    return Page();
                }

                User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
            }
            
            if (IsNewUser)
            {
                await _userService.CreateUserAsync(User);
            }
            else
            {
                await _userService.UpdateUserAsync(User);
            }

            return RedirectToPage("/admin/users");
        }
    }
}
