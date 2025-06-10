using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Text.Json;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public PreOrderConfirmationModel(
            IOrderService orderService,
            ApplicationDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int TableNumber { get; set; }
        public int NumberOfGuests { get; set; }
        public string Note { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new();
        public decimal TotalAmount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get reservation details from TempData
            if (!TempData.ContainsKey("ReservationDate") || !TempData.ContainsKey("ReservationTime") ||
                !TempData.ContainsKey("TableId") || !TempData.ContainsKey("NumberOfGuests"))
            {
                return RedirectToPage("/Reservation");
            }

            ReservationDate = DateTime.Parse(TempData["ReservationDate"].ToString());
            ReservationTime = TimeSpan.Parse(TempData["ReservationTime"].ToString());
            var tableId = int.Parse(TempData["TableId"].ToString());
            NumberOfGuests = int.Parse(TempData["NumberOfGuests"].ToString());
            Note = TempData["Note"]?.ToString();

            // Get table number
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return RedirectToPage("/Reservation");
            }
            TableNumber = table.TableNumber;

            // Process selected items if any
            if (TempData.ContainsKey("SelectedItems") && !string.IsNullOrEmpty(TempData["SelectedItems"]?.ToString()))
            {
                var selectedItemsJson = TempData["SelectedItems"].ToString();
                var selectedItems = JsonSerializer.Deserialize<Dictionary<string, int>>(selectedItemsJson);

                if (selectedItems != null && selectedItems.Any())
                {
                    foreach (var item in selectedItems)
                    {
                        if (int.TryParse(item.Key, out int menuItemId))
                        {
                            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
                            if (menuItem != null)
                            {
                                OrderItems.Add(new OrderItemViewModel
                                {
                                    MenuItemId = menuItem.Id,
                                    Name = menuItem.Name,
                                    Price = menuItem.Price,
                                    Quantity = item.Value,
                                    Subtotal = menuItem.Price * item.Value
                                });
                            }
                        }
                    }
                }
            }

            // Calculate total amount
            TotalAmount = OrderItems.Sum(i => i.Subtotal);

            // Create the reservation and order in the database
            await CreateReservationAndOrder(tableId);

            return Page();
        }

        private async Task CreateReservationAndOrder(int tableId)
        {
            try
            {
                // Create reservation
                var reservation = new Reservation
                {
                    UserId = User.Identity.IsAuthenticated ? int.Parse(User.FindFirst("UserId")?.Value) : 1, // Guest user if not authenticated
                    TableId = tableId,
                    ReservationTime = ReservationDate.Add(ReservationTime),
                    Note = Note
                };

                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();

                // Create order if there are menu items
                if (OrderItems.Any())
                {
                    var order = new Order
                    {
                        UserId = reservation.UserId,
                        TableId = tableId,
                        ReservationTime = reservation.ReservationTime,
                        NumberOfGuests = NumberOfGuests,
                        Status = "Confirmed",
                        OrderDate = DateTime.Now,
                        TotalPrice = TotalAmount,
                        Note = Note
                    };

                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    // Add order items
                    foreach (var item in OrderItems)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price
                        };

                        await _context.OrderItems.AddAsync(orderItem);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error creating reservation: {ex.Message}");
            }
        }
    }

    public class OrderItemViewModel
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
} 