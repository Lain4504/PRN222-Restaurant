using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace PRN222_Restaurant.Pages
{
    [Authorize]
    public class OrderListModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private const int DefaultPageSize = 10;

        public OrderListModel(IOrderService orderService, ApplicationDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public PagedResult<Models.Order> OrdersResult { get; set; } = new PagedResult<Models.Order>();
        public List<Models.Order> Orders => OrdersResult.Items;

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DefaultPageSize;

        public async Task OnGetAsync()
        {
            // Ensure valid pagination parameters
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = DefaultPageSize;

            // Get current user ID
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            // Get paginated orders for the current user
            OrdersResult = await _orderService.GetPagedUserOrdersAsync(userId, CurrentPage, PageSize);
        }

        public async Task<IActionResult> OnGetOrderDetailsAsync(int id)
        {
            // Get current user ID
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            // Get order details
            var order = await _orderService.GetOrderByIdAsync(id);

            // Check if order exists and belongs to the current user
            if (order == null || order.UserId != userId)
            {
                return NotFound();
            }

            return new JsonResult(new
            {
                order.Id,
                order.OrderType,
                order.Status,
                order.OrderDate,
                order.ReservationTime,
                order.TotalPrice,
                order.Note,
                order.NumberOfGuests,
                Table = order.Table?.TableNumber,
                Items = order.OrderItems.Select(item => new
                {
                    item.MenuItem.Name,
                    item.Quantity,
                    item.UnitPrice,
                    Total = item.Quantity * item.UnitPrice
                })
            });
        }
    }
} 