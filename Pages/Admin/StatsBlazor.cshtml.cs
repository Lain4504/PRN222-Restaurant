using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Helper;

namespace PRN222_Restaurant.Pages.Admin
{
    [Authorize]
    public class StatsBlazorModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Check if current user is Admin
            if (!AuthHelper.IsAdmin(HttpContext.User))
            {
                return Redirect("/admin/login");
            }

            // This page just hosts the Blazor component
            return Page();
        }
    }
}
