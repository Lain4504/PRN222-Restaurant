using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_Restaurant.Pages.Admin
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Sign out user
            // For example, if using Identity:
            // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Redirect to login page
            return RedirectToPage("/admin/login");
        }
    }
}
