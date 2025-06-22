using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Pages;
using PRN222_Restaurant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly IVNPayService _vnPayService;
        private readonly ApplicationDbContext _context;
        private readonly IFeedbackService _feedbackService;
        public CheckoutModel(IVNPayService vnPayService, ApplicationDbContext context, IFeedbackService feedbackService)
        {
            _vnPayService = vnPayService;
            _context = context;
            _feedbackService = feedbackService;
        }

        [BindProperty]
        public PaymentInformation PaymentInfo { get; set; }

        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public int OrderId { get; set; }
        [BindProperty]
        public Feedback NewFeedback { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int? orderId)
        {
            // If orderId is provided, load that specific order
            if (orderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToPage("/Error");
                }

                // Store order ID in session for later use
                HttpContext.Session.SetInt32("CurrentOrderId", order.Id);
                OrderId = order.Id;

                // Load order items
                await LoadOrderItems(order);

                return Page();
            }

            // If no orderId provided, check if there's a current order in session
            var sessionOrderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (sessionOrderId.HasValue)
            {
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                    .FirstOrDefaultAsync(o => o.Id == sessionOrderId.Value);

                if (existingOrder != null)
                {
                    OrderId = existingOrder.Id;

                    // Load order items
                    await LoadOrderItems(existingOrder);

                    return Page();
                }
            }

            // If no valid order was found, create a test order (for demo purposes only)
            var testOrder = new Order
            {
                UserId = 1, // Test user
                TableId = 1, // Test table
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalPrice = 0,
                OrderItems = new List<OrderItem>()
            };

            _context.Orders.Add(testOrder);
            await _context.SaveChangesAsync();

            // Add test menu items to order
            var menuItems = await _context.MenuItems.Take(2).ToListAsync();
            foreach (var item in menuItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = testOrder.Id,
                    MenuItemId = item.Id,
                    Quantity = 1,
                    UnitPrice = item.Price
                };
                _context.OrderItems.Add(orderItem);
                testOrder.TotalPrice += item.Price;
            }

            await _context.SaveChangesAsync();

            // Store order ID in session for later use
            HttpContext.Session.SetInt32("CurrentOrderId", testOrder.Id);
            OrderId = testOrder.Id;

            // Reload the order with items
            var reloadedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == testOrder.Id);

            if (reloadedOrder != null)
            {
                await LoadOrderItems(reloadedOrder);
            }

            return Page();
        }

        private async Task LoadOrderItems(Order order)
        {
            OrderItems.Clear();
            Subtotal = 0;

            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    var menuItem = item.MenuItem ?? await _context.MenuItems.FindAsync(item.MenuItemId);

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
                        Subtotal += orderItem.Subtotal;
                    }
                }
            }

            // Calculate tax and total
            Tax = Math.Round(Subtotal * 0.1m, 2);
            Total = Subtotal + Tax;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId == null)
            {
                return RedirectToPage("/Error");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return RedirectToPage("/Error");
            }

            // Create payment information
            PaymentInfo = new PaymentInformation
            {
                OrderId = order.Id.ToString(),
                Amount = order.TotalPrice,
                OrderDescription = $"Payment for reservation #{order.Id}",
                OrderType = "PreOrder",
                Name = User.Identity?.Name ?? "Guest"
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(PaymentInfo, HttpContext);
            return Redirect(paymentUrl);
        }

        // VNPay callback: /checkout/payment-callback
        public async Task<IActionResult> OnGetPaymentCallbackAsync()
        {
            var response = _vnPayService.ProcessPaymentCallback(Request.Query);
            // Lấy orderId từ vnp_OrderInfo hoặc vnp_TxnRef hoặc session
            int? orderId = null;
            if (Request.Query.ContainsKey("vnp_OrderInfo"))
            {
                var info = Request.Query["vnp_OrderInfo"].ToString();
                var idStr = info.Split('#').LastOrDefault()?.Trim();
                if (int.TryParse(idStr, out int parsedId)) orderId = parsedId;
            }
            if (orderId == null)
            {
                orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            }
            if (orderId != null)
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order != null)
                {
                    if (response.Success)
                    {
                        // Create payment record
                        var payment = new Payment
                        {
                            OrderId = order.Id,
                            Amount = order.TotalPrice,
                            PaymentDate = DateTime.Now,
                            Status = "Paid"
                        };
                        _context.Payments.Add(payment);
                        // Update order status
                        order.Status = "Paid";
                        await _context.SaveChangesAsync();
                        TempData["PaymentSuccess"] = true;
                        TempData["PaymentMessage"] = "Payment successful!";
                        TempData["TransactionId"] = response.TransactionId;
                        return RedirectToPage("/PaymentSuccess");
                    }
                    else
                    {
                        order.Status = "PaymentFailed";
                        await _context.SaveChangesAsync();
                        TempData["PaymentSuccess"] = false;
                        TempData["PaymentMessage"] = "Payment failed!";
                        TempData["PaymentError"] = response.VnPayResponseCode;
                        return RedirectToPage("/PaymentFailed");
                    }
                }
            }
            return RedirectToPage("/Error");
        }



        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }


        public async Task<IActionResult> OnPostSubmitFeedbackAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                TempData["FeedbackError"] = "Bạn phải đăng nhập để gửi phản hồi.";
                return Page();
            }

            // Kiểm tra xem user có tồn tại không (tránh lỗi khóa ngoại)
            if (!await _context.Users.AnyAsync(u => u.Id == userId.Value))
            {
                TempData["FeedbackError"] = "Người dùng không hợp lệ.";
                return Page();
            }

            NewFeedback.UserId = userId.Value;
            NewFeedback.CreatedAt = DateTime.Now;

            try
            {
                await _feedbackService.AddAsync(NewFeedback);
                TempData["FeedbackSuccess"] = "Phản hồi của bạn đã được ghi nhận.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["FeedbackError"] = "Có lỗi xảy ra khi lưu phản hồi: " + ex.Message;
                return Page();
            }



        }
    }
}