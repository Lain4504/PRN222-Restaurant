using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class OrdersModel : PageModel
    {
        private readonly IOrderService _orderService;

        public OrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public List<PRN222_Restaurant.Models.Order> Orders { get; set; } = new List<PRN222_Restaurant.Models.Order>();

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public string OrderStatus { get; set; }

        [BindProperty]
        public int OrderId { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy tất cả đơn hàng, không lọc theo userId
            var orders = await _orderService.GetAllOrdersAsync();
            Orders = orders;
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            if (string.IsNullOrEmpty(OrderStatus) || OrderId <= 0)
            {
                StatusMessage = "Error: Invalid order information";
                return RedirectToPage("/admin/orders", null, "admin/orders");
            }

            var success = await _orderService.UpdateOrderStatusAsync(OrderId, OrderStatus);

            if (success)
            {
                StatusMessage = $"Order #{OrderId} status updated to {OrderStatus}";
            }
            else
            {
                StatusMessage = $"Error: Failed to update order #{OrderId}";
            }

            // Always redirect to lowercase URL
            return Redirect("/admin/orders");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _orderService.CancelOrderAsync(id);

            if (success)
            {
                StatusMessage = $"Order #{id} has been cancelled";
            }
            else
            {
                StatusMessage = $"Error: Failed to cancel order #{id}";
            }

            // Always redirect to lowercase URL
            return Redirect("/admin/orders");
        }
    }
} 