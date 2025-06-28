using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Collections.Generic;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly IPointsService _pointsService;

        public PreOrderConfirmationModel(IOrderService orderService, ApplicationDbContext context, IPointsService pointsService)
        {
            _orderService = orderService;
            _context = context;
            _pointsService = pointsService;
        }

        public Order? Order { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int TableNumber { get; set; }
        public int NumberOfGuests { get; set; }
        public string? Note { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public decimal TotalAmount { get; set; }

        // Points-related properties
        public int UserPoints { get; set; }
        public int MaxUsablePoints { get; set; }
        [BindProperty]
        public int PointsToUse { get; set; }
        public decimal PointsDiscount { get; set; }
        public decimal FinalTotal { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Console.WriteLine($"PreOrderConfirmation: Loading order with ID {id}");
                
                // Fetch order details
                Order = await _orderService.GetOrderWithItemsAsync(id);
                if (Order == null)
                {
                    Console.WriteLine($"PreOrderConfirmation: Order with ID {id} not found");
                    return RedirectToPage("/Index");
                }

                Console.WriteLine($"PreOrderConfirmation: Order loaded successfully - OrderType: {Order.OrderType}, Status: {Order.Status}");

                // Set reservation details
                if (Order.ReservationTime.HasValue)
                {
                    ReservationDate = Order.ReservationTime.Value.Date;
                    ReservationTime = Order.ReservationTime.Value.TimeOfDay;
                }
                else
                {
                    // Fallback values if reservation time is null
                    ReservationDate = DateTime.Now.Date;
                    ReservationTime = DateTime.Now.TimeOfDay;
                }
                
                Note = Order.Note;
                NumberOfGuests = (int)Order.NumberOfGuests!;

                // Get table details
                var table = await _context.Tables.FindAsync(Order.TableId);
                TableNumber = table?.TableNumber ?? 0;

                // Get order items
                OrderItems = new List<OrderItemViewModel>();
                TotalAmount = 0;

                if (Order.OrderItems != null && Order.OrderItems.Any())
                {
                    foreach (var item in Order.OrderItems)
                    {
                        var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                        if (menuItem != null)
                        {
                            var orderItem = new OrderItemViewModel
                            {
                                Id = item.Id,
                                Name = menuItem.Name,
                                Price = menuItem.Price,
                                Quantity = item.Quantity,
                                Subtotal = menuItem.Price * item.Quantity
                            };

                            OrderItems.Add(orderItem);
                            TotalAmount += orderItem.Subtotal;
                        }
                    }
                }
                
                Console.WriteLine($"PreOrderConfirmation: Loaded {OrderItems.Count} order items with total amount: {TotalAmount}");

                // Load points information for authenticated users
                await LoadPointsInformationAsync();
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in PreOrderConfirmation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Initialize empty collections
                OrderItems = new List<OrderItemViewModel>();
                TotalAmount = 0;
                FinalTotal = 0;

                // Redirect to home page on severe errors
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSetPointsAsync(int id)
        {
            // Reload order data
            await OnGetAsync(id);

            if (Order == null)
            {
                return RedirectToPage("/Index");
            }

            // Validate points selection but don't redeem yet
            if (User.Identity?.IsAuthenticated == true && PointsToUse > 0)
            {
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    // Validate points redemption
                    var isValid = await _pointsService.ValidatePointsRedemptionAsync(userId.Value, PointsToUse);
                    if (isValid && PointsToUse <= MaxUsablePoints)
                    {
                        // Store points selection in session for checkout
                        HttpContext.Session.SetInt32("PointsToUse", PointsToUse);
                        TempData["SuccessMessage"] = $"Selected {PointsToUse} points for ${(PointsToUse * 1.0m):F2} discount. Points will be applied during payment.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid points amount. Please check your available points.";
                        PointsToUse = 0;
                    }
                }
            }
            else
            {
                // Clear points selection
                HttpContext.Session.Remove("PointsToUse");
                PointsToUse = 0;
            }

            // Reload points information
            await LoadPointsInformationAsync();

            return Page();
        }

        private async Task LoadPointsInformationAsync()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = GetCurrentUserId();
                if (userId.HasValue)
                {
                    UserPoints = await _pointsService.GetUserPointsAsync(userId.Value);
                    MaxUsablePoints = await _pointsService.GetMaxUsablePointsAsync(userId.Value, TotalAmount);

                    // Check if there's a stored points selection from session
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

                    FinalTotal = TotalAmount - PointsDiscount;
                }
            }
            else
            {
                FinalTotal = TotalAmount;
            }
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