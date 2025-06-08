using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services.IService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class OrdersModel : PageModel
    {
        private readonly IOrderService _orderService;
        private const int DefaultPageSize = 10;

        public OrdersModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public PagedResult<PRN222_Restaurant.Models.Order> OrdersResult { get; set; } = new PagedResult<PRN222_Restaurant.Models.Order>();
        public List<PRN222_Restaurant.Models.Order> Orders => OrdersResult.Items;

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public string OrderStatus { get; set; }

        [BindProperty]
        public int OrderId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DefaultPageSize;

        public async Task OnGetAsync()
        {
            // Ensure valid pagination parameters
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = DefaultPageSize;
            
            // Get paginated orders
            OrdersResult = await _orderService.GetPagedOrdersAsync(CurrentPage, PageSize);
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            if (string.IsNullOrEmpty(OrderStatus) || OrderId <= 0)
            {
                StatusMessage = "Error: Invalid order information";
                return RedirectToPage("/admin/orders", new { CurrentPage, PageSize });
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

            // Redirect to the same page with pagination parameters
            return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
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

            // Redirect to the same page with pagination parameters
            return Redirect($"/admin/orders?currentPage={CurrentPage}&pageSize={PageSize}");
        }
    }
} 