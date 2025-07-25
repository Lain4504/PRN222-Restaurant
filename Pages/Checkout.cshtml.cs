using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Pages;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Services.IService;
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
        private readonly IPointsService _pointsService;

        public CheckoutModel(IVNPayService vnPayService, ApplicationDbContext context, IPointsService pointsService)
        {
            _vnPayService = vnPayService;
            _context = context;
            _pointsService = pointsService;
        }

        [BindProperty]
        public PaymentInformation PaymentInfo { get; set; }

        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public int OrderId { get; set; }

        // Points-related properties
        public int UserPoints { get; set; }
        public int MaxUsablePoints { get; set; }
        [BindProperty]
        public int PointsToUse { get; set; }
        public decimal PointsDiscount { get; set; }
        public decimal FinalTotal { get; set; }
        public decimal PointValue { get; set; }
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

            // Update table status to Pending when order is created
            if (testOrder.TableId.HasValue && testOrder.TableId.Value > 0)
            {
                var table = await _context.Tables.FindAsync(testOrder.TableId.Value);
                if (table != null)
                {
                    table.Status = "Pending";
                    await _context.SaveChangesAsync();
                }
            }

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

            // Total is same as subtotal (no tax)
            Total = Subtotal;

            // Load points information for authenticated users
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    UserPoints = await _pointsService.GetUserPointsAsync(userId.Value);
                    MaxUsablePoints = await _pointsService.GetMaxUsablePointsAsync(userId.Value, Total);

                    // Check if there's a stored points selection from PreOrderConfirmation
                    var storedPoints = HttpContext.Session.GetInt32("PointsToUse");
                    if (storedPoints.HasValue && PointsToUse == 0)
                    {
                        PointsToUse = storedPoints.Value;
                    }

                    // Calculate points discount if points are being used
                    if (PointsToUse > 0 && PointsToUse <= MaxUsablePoints)
                    {
                        PointsDiscount = await _pointsService.CalculatePointsDiscountAsync(PointsToUse);
                    }

                    FinalTotal = Total - PointsDiscount;
                }
            }

            // Get point value from config for JavaScript calculations
            var pointsConfig = _pointsService.GetPointsConfig();
            PointValue = pointsConfig.PointValue;

            // Set final total if not authenticated
            if (!User.Identity.IsAuthenticated)
            {
                FinalTotal = Total;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"OnPostAsync - PointsToUse received: {PointsToUse}");

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

            // If PaymentInfo.Amount is provided from form (JavaScript calculated), use it directly
            decimal paymentAmount;
            string paymentDescription = $"Deposit payment (20%) for reservation #{order.Id}";
            string paymentType = "Deposit";

            if (PaymentInfo?.Amount > 0)
            {
                // Use the amount calculated by JavaScript (already includes points discount and 20% calculation)
                paymentAmount = PaymentInfo.Amount;
                Console.WriteLine($"Using payment amount from form (JavaScript): {paymentAmount}");

                // Store points to use in order for payment callback
                if (PointsToUse > 0 && User.Identity.IsAuthenticated)
                {
                    order.PointsUsed = PointsToUse;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Stored points to use in order: {PointsToUse}");
                }
            }
            else
            {
                // Fallback: calculate manually
                await LoadOrderItems(order);
                decimal finalAmount = Total;

                if (PointsToUse > 0 && User.Identity.IsAuthenticated)
                {
                    // Store points to use in order for payment callback
                    order.PointsUsed = PointsToUse;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    // Calculate discount for payment amount calculation only
                    var pointsDiscount = await _pointsService.CalculatePointsDiscountAsync(PointsToUse);
                    finalAmount = Math.Max(0, Total - pointsDiscount);
                    Console.WriteLine($"Calculated final amount with points discount: {finalAmount}");
                }

                paymentAmount = finalAmount * 0.2m; // 20% deposit
                Console.WriteLine($"Using calculated payment amount: {paymentAmount}");
            }

            // Create payment information
            PaymentInfo = new PaymentInformation
            {
                OrderId = order.Id.ToString(),
                Amount = paymentAmount,
                OrderDescription = paymentDescription,
                OrderType = order.OrderType,
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
                        // Determine payment type and amount based on order type
                        decimal paidAmount;
                        string paymentType;
                        string orderStatus;

                        if (order.OrderType == "PreOrder")
                        {
                            paidAmount = order.TotalPrice * 0.2m; // 20% deposit
                            paymentType = "Deposit";
                            orderStatus = "Paid Deposit";
                        }
                        else
                        {
                            paidAmount = order.TotalPrice; // Full payment
                            paymentType = "Full";
                            orderStatus = "Paid";
                        }

                        // Create payment record
                        var payment = new Payment
                        {
                            OrderId = order.Id,
                            Amount = paidAmount,
                            PaymentDate = DateTime.Now,
                            Status = "Paid",
                            PaymentType = paymentType,
                            DepositPercentage = order.OrderType == "PreOrder" ? 0.2m : null
                        };
                        _context.Payments.Add(payment);

                        // Redeem points if any were selected and stored in order
                        if (order.PointsUsed.HasValue && order.PointsUsed.Value > 0 && order.UserId.HasValue)
                        {
                            var pointsDiscount = await _pointsService.RedeemPointsAsync(order.UserId.Value, order.PointsUsed.Value, order.Id, "Points redemption for order");
                            Console.WriteLine($"Points redeemed after successful payment: {order.PointsUsed.Value}, Discount: {pointsDiscount}");

                            if (pointsDiscount > 0)
                            {
                                // Update order total price to reflect discount
                                var newTotal = Math.Max(0, order.TotalPrice - pointsDiscount);
                                order.TotalPrice = newTotal;
                                _context.Orders.Update(order);

                                // Update payment amount if it was a deposit
                                if (paymentType == "Deposit")
                                {
                                    payment.Amount = newTotal * 0.2m; // Recalculate 20% deposit
                                    _context.Payments.Update(payment);
                                }
                            }
                        }

                        // Update order status
                        order.Status = orderStatus;
                        await _context.SaveChangesAsync();

                        // Award points only for full payment, not deposit
                        if (order.UserId.HasValue && paymentType == "Full")
                        {
                            await _pointsService.AwardPointsAsync(order.UserId.Value, order.Id, order.TotalPrice, "Order payment");

                            // Check and award welcome bonus for new users
                            if (await _pointsService.IsEligibleForWelcomeBonusAsync(order.UserId.Value))
                            {
                                await _pointsService.AwardWelcomeBonusAsync(order.UserId.Value);
                            }
                        }

                        TempData["PaymentSuccess"] = true;
                        TempData["PaymentMessage"] = "Payment successful!";
                        TempData["TransactionId"] = response.TransactionId;
                        return RedirectToPage("/PaymentSuccess");
                    }
                    else
                    {
                        order.Status = "PaymentFailed";

                        // Reset points used on payment failure
                        order.PointsUsed = null;
                        _context.Orders.Update(order);
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



    }
}