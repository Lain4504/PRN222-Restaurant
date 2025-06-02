using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly IVNPayService _vnPayService;
        private readonly ApplicationDbContext _context;

        public CheckoutModel(IVNPayService vnPayService, ApplicationDbContext context)
        {
            _vnPayService = vnPayService;
            _context = context;
        }

        [BindProperty]
        public PaymentInformation PaymentInfo { get; set; }

        public async Task OnGetAsync()
        {
            // Create a test order with table and menu items
            var testOrder = new Order
            {
                UserId = 1, // Test user
                TableId = 1, // Test table
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalPrice = 0
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
                OrderDescription = $"Thanh toan don hang #{order.Id}",
                OrderType = "other",
                Name = "Khách hàng"
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
                        TempData["PaymentMessage"] = "Thanh toán thành công!";
                        TempData["TransactionId"] = response.TransactionId;
                        return RedirectToPage("/PaymentSuccess");
                    }
                    else
                    {
                        order.Status = "PaymentFailed";
                        await _context.SaveChangesAsync();
                        TempData["PaymentSuccess"] = false;
                        TempData["PaymentMessage"] = "Thanh toán thất bại!";
                        TempData["PaymentError"] = response.VnPayResponseCode;
                        return RedirectToPage("/PaymentFailed");
                    }
                }
            }
            return RedirectToPage("/Error");
        }
    }
}
