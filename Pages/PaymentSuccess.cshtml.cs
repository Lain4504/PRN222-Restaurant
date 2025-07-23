using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Pages;
using PRN222_Restaurant.Helpers;
using PRN222_Restaurant.Services.IService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages
{
    public class PaymentSuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        private readonly IPointsService _pointsService;

        public PaymentSuccessModel(ApplicationDbContext context, NotificationHelper notificationHelper, IPointsService pointsService)
        {
            _context = context;
            _notificationHelper = notificationHelper;
            _pointsService = pointsService;
        }
        
        public Order? Order { get; set; }
        public int TableNumber { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public decimal TotalAmount { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal PointsDiscount { get; set; }
        public int PointsUsed { get; set; }
        public int PointsEarned { get; set; }
        public int TotalPoints { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the order ID from session
            var orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId.HasValue)
            {
                // Load the order with related data
                Order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value);

                if (Order != null)
                {
                    // Get table number
                    if (Order.TableId.HasValue)
                    {
                        var table = await _context.Tables.FindAsync(Order.TableId.Value);
                        TableNumber = table?.TableNumber ?? 0;
                    }

                    // Load order items and calculate original amount
                    await LoadOrderItems(Order);

                    // Calculate original amount from order items (before discount)
                    OriginalAmount = OrderItems.Sum(item => item.Subtotal);

                    // Calculate points discount if any
                    if (OriginalAmount > Order.TotalPrice)
                    {
                        PointsDiscount = OriginalAmount - Order.TotalPrice;

                        // Calculate points used (approximate)
                        var pointsConfig = _pointsService.GetPointsConfig();
                        PointsUsed = (int)(PointsDiscount / pointsConfig.PointValue);
                    }

                    // Load points information
                    if (User.Identity.IsAuthenticated && Order.UserId.HasValue)
                    {
                        PointsEarned = await _pointsService.CalculatePointsEarnedAsync(Order.TotalPrice);
                        TotalPoints = await _pointsService.GetUserPointsAsync(Order.UserId.Value);

                        // Create payment success notification
                        await _notificationHelper.NotifyPaymentSuccessAsync(Order.UserId.Value, Order.Id, Order.TotalPrice);
                    }
                }
            }

            return Page();
        }
        
        private async Task LoadOrderItems(Order order)
        {
            OrderItems.Clear();

            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);

                    if (menuItem != null)
                    {
                        var orderItem = new OrderItemViewModel
                        {
                            Id = item.Id,
                            Name = menuItem.Name,
                            Price = item.UnitPrice > 0 ? item.UnitPrice : menuItem.Price,
                            Quantity = item.Quantity,
                            Subtotal = (item.UnitPrice > 0 ? item.UnitPrice : menuItem.Price) * item.Quantity
                        };

                        OrderItems.Add(orderItem);
                    }
                }
            }

            // Use Order.TotalPrice which includes any discounts applied
            TotalAmount = order.TotalPrice;
        }
    }
} 