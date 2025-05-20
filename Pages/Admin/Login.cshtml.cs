using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_Restaurant.Pages.Admin
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public bool RememberMe { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Just render the form
        }

        public IActionResult OnPost()
        {
            // In a real application, you would validate against a database
            // For demo purposes, we'll use a hardcoded credential            if (Username == "admin" && Password == "admin123")
            {
                // In a real application, you would set authentication cookie
                
                return RedirectToPage("/admin/dashboard");
            }

            ErrorMessage = "Invalid username or password";
            return Page();
        }
    }
}
