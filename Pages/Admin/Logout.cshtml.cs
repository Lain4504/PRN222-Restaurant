using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_Restaurant.Pages.Admin
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Xoá token khỏi Session
            HttpContext.Session.Remove("AuthToken");


            var role = HttpContext.Session.GetString("UserRole");

            if (role == "Admin" || role == "Staff")
            {
                return Redirect("/admin/login");
            }
            else
            {
                return Redirect("/index");
            }
        }
    }
}