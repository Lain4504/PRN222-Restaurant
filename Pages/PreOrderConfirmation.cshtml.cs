using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;

        public PreOrderConfirmationModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public Order Order { get; set; }
        public int TableNumber { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _orderService.GetOrderByIdAsync(id);

            if (Order == null)
            {
                return NotFound();
            }

            // Verify that the user is authorized to view this order
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            if (Order.UserId != userId)
            {
                return Forbid();
            }

            TableNumber = Order.Table.TableNumber;

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            await _orderService.CancelOrderAsync(id);
            return RedirectToPage("/Index");
        }
    }
} 