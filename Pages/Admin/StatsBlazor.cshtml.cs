using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_Restaurant.Pages.Admin
{
    [Authorize]
    public class StatsBlazorModel : PageModel
    {
        public void OnGet()
        {
            // This page just hosts the Blazor component
        }
    }
}
