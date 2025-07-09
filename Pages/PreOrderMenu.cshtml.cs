using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using PRN222_Restaurant.Data;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderMenuModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly NotificationHelper _notificationHelper;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IReservationSessionService _reservationSessionService;
        private readonly IPointsService _pointsService;

        public PreOrderMenuModel(
            IOrderService orderService,
            ApplicationDbContext context,
            NotificationHelper notificationHelper,
            IReservationSessionService reservationSessionService,
            IPointsService pointsService)
        {
            _orderService = orderService;
            _context = context;
            _notificationHelper = notificationHelper;
            _reservationSessionService = reservationSessionService;
            _pointsService = pointsService;
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

        public decimal FinalTotal { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Check for saved reservation data first
                var savedReservationData = _reservationSessionService.GetReservationData();
                if (savedReservationData != null)
                {
                    // Create order from saved reservation data
                    await CreateOrderFromReservationData(savedReservationData);
                    // Clear the saved data after using it
                    _reservationSessionService.ClearReservationData();
                }
                else
                {
                    // Load order from TempData or Session
                    await LoadCurrentOrder();
                }

                if (CurrentOrder == null)
                {
                    StatusMessage = "No order found. Please create a reservation first.";
                    return RedirectToPage("/Reservation");
                }



                // Calculate totals
                CalculateTotals();

                // Load points information
                await LoadPointsInformationAsync();

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
            // Check if user is authenticated first
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect to login page
                return Redirect("/login");
            }

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

                // Points will be handled in checkout page

                // Create notification for order creation
                if (User.Identity.IsAuthenticated && CurrentOrder.UserId.HasValue)
                {
                    await _notificationHelper.NotifyOrderCreatedAsync(CurrentOrder.UserId.Value, CurrentOrder.Id);
                }

                // Redirect directly to checkout page
                return RedirectToPage("/Checkout", new { orderId = CurrentOrder.Id });
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

        private async Task CreateOrderFromReservationData(ReservationSessionData reservationData)
        {
            // Create order from saved reservation data
            var order = new Order
            {
                UserId = User.Identity.IsAuthenticated ? int.Parse(User.FindFirst("UserId")?.Value ?? "1") : 1,
                TableId = reservationData.TableId,
                OrderDate = DateTime.Now,
                ReservationTime = reservationData.ReservationDate.Date.Add(reservationData.ReservationTime),
                NumberOfGuests = reservationData.NumberOfGuests,
                Note = reservationData.Note,
                OrderType = "PreOrder",
                Status = "Pending",
                OrderItems = new List<OrderItem>()
            };

            // Parse selected items if any
            if (!string.IsNullOrEmpty(reservationData.SelectedItems) && reservationData.SelectedItems != "{}")
            {
                try
                {
                    var selectedItemsDict = JsonSerializer.Deserialize<Dictionary<int, int>>(reservationData.SelectedItems);
                    if (selectedItemsDict != null)
                    {
                        foreach (var item in selectedItemsDict)
                        {
                            var menuItem = await _context.MenuItems.FindAsync(item.Key);
                            if (menuItem != null && item.Value > 0)
                            {
                                order.OrderItems.Add(new OrderItem
                                {
                                    MenuItemId = item.Key,
                                    Quantity = item.Value,
                                    UnitPrice = menuItem.Price
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing selected items: {ex.Message}");
                }
            }

            // Update table status to Pending
            var table = await _context.Tables.FindAsync(reservationData.TableId);
            if (table != null)
            {
                table.Status = "Pending";
            }

            // Calculate and set total price before saving
            order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

            // Save order to database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Set current order
            CurrentOrder = order;

            // Save OrderId to TempData and Session for future requests
            TempData["OrderId"] = order.Id.ToString();
            TempData.Keep("OrderId");
            HttpContext.Session.SetString("OrderId", order.Id.ToString());

            // Calculate totals for display
            TotalPrice = order.TotalPrice;
            TotalItems = order.OrderItems.Sum(oi => oi.Quantity);
        }

        private async Task LoadPointsInformationAsync()
        {
            // Points will be handled in checkout page
            FinalTotal = TotalPrice;
        }

        private int? GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }

    }
}