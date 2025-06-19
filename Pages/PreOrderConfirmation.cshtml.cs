using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Collections.Generic;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;

        public PreOrderConfirmationModel(IOrderService orderService, ApplicationDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public Order? Order { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int TableNumber { get; set; }
        public int NumberOfGuests { get; set; }
        public string? Note { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public decimal TotalAmount { get; set; }

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
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error in PreOrderConfirmation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Initialize empty collections
                OrderItems = new List<OrderItemViewModel>();
                TotalAmount = 0;
                
                // Redirect to home page on severe errors
                return RedirectToPage("/Index");
            }

            return Page();
        }
    }
} 