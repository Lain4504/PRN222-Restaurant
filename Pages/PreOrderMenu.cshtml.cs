using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using PRN222_Restaurant.Data;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderMenuModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        private readonly JsonSerializerOptions _jsonOptions;

        public PreOrderMenuModel(
            IOrderService orderService,
            ApplicationDbContext context,
            NotificationHelper notificationHelper)
        {
            _orderService = orderService;
            _context = context;
            _notificationHelper = notificationHelper;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 64
            };
        }

        public Order? CurrentOrder { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public string StatusMessage { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Load order from TempData or Session
                await LoadCurrentOrder();

                if (CurrentOrder == null)
                {
                    StatusMessage = "No order found. Please create a reservation first.";
                    return RedirectToPage("/Reservation");
                }



                // Calculate totals
                CalculateTotals();

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnGetAsync: {ex.Message}");
                StatusMessage = "Error loading order details.";
                return RedirectToPage("/Reservation");
            }
        }



        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Load current order
                await LoadCurrentOrder();

                if (CurrentOrder == null)
                {
                    StatusMessage = "Order not found.";
                    return RedirectToPage("/Reservation");
                }

                if (!CurrentOrder.OrderItems.Any())
                {
                    StatusMessage = "Please add at least one item to your order.";
                    return Page();
                }

                // Create notification for order creation
                if (User.Identity.IsAuthenticated && CurrentOrder.UserId.HasValue)
                {
                    await _notificationHelper.NotifyOrderCreatedAsync(CurrentOrder.UserId.Value, CurrentOrder.Id);
                }

                // Redirect to confirmation page with the order id
                return RedirectToPage("/PreOrderConfirmation", new { id = CurrentOrder.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing order: {ex.Message}");
                StatusMessage = "An error occurred while processing your order.";
                return Page();
            }
        }
        

        




        private async Task LoadCurrentOrder()
        {
            try
            {
                // Try to get OrderId from TempData first
                string? orderIdStr = TempData["OrderId"]?.ToString();

                // If not in TempData, try Session
                if (string.IsNullOrEmpty(orderIdStr))
                {
                    orderIdStr = HttpContext.Session.GetString("OrderId");
                }

                if (!string.IsNullOrEmpty(orderIdStr) && int.TryParse(orderIdStr, out int orderId))
                {
                    // Load order with related data
                    CurrentOrder = await _context.Orders
                        .Include(o => o.Table)
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.MenuItem)
                                .ThenInclude(mi => mi.Category)
                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (CurrentOrder != null)
                    {
                        Console.WriteLine($"Loaded order {CurrentOrder.Id} with {CurrentOrder.OrderItems.Count} items");

                        // Keep OrderId in TempData for future requests
                        TempData["OrderId"] = orderId.ToString();
                        TempData.Keep("OrderId");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading current order: {ex.Message}");
                CurrentOrder = null;
            }
        }

        private void CalculateTotals()
        {
            if (CurrentOrder?.OrderItems != null)
            {
                TotalItems = CurrentOrder.OrderItems.Sum(oi => oi.Quantity);
                TotalPrice = CurrentOrder.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
            }
            else
            {
                TotalItems = 0;
                TotalPrice = 0;
            }
        }


    }
}