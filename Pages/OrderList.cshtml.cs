using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Services.IService;
using Microsoft.AspNetCore.Authorization;

namespace PRN222_Restaurant.Pages
{
    [Authorize]
    public class OrderListModel : PageModel
    {
        private readonly IOrderService _orderService;

        public OrderListModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Admin/Login");
            }

            return Page();
        }
    }
} 