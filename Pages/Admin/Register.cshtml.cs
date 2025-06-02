using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class RegisterModel : PageModel
    {
        private readonly IUserService _userService;

        public RegisterModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public class InputModel
        {
            [Required(ErrorMessage = "Full Name is required")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid Email address")]
            public string Email { get; set; } = string.Empty;

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fix the errors below.";
                return Page();
            }

            var existingUser = await _userService.GetUserByEmailAsync(Input.Email);
            if (existingUser != null)
            {
                ErrorMessage = "Email is already registered.";
                return Page();
            }

            var newUser = new User
            {
                FullName = Input.FullName,
                Email = Input.Email,
                Role = "Customer",
                IsActive = true
            };

            var success = await _userService.CreateUserAsync(newUser);
            if (!success)
            {
                ErrorMessage = "Failed to register user.";
                return Page();
            }
            SuccessMessage = "Registration successful! You can now login.";
            return RedirectToPage("/Admin/Login");

        }
    }
}
