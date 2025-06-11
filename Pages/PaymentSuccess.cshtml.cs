using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class PaymentSuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PaymentSuccessModel(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public Order? Order { get; set; }
        public int TableNumber { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public decimal TotalAmount { get; set; }

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
                    
                    // Load order items
                    await LoadOrderItems(Order);
                }
            }
            
            return Page();
        }
        
        private async Task LoadOrderItems(Order order)
        {
            OrderItems.Clear();
            TotalAmount = 0;
            
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
                        TotalAmount += orderItem.Subtotal;
                    }
                }
            }
        }
    }
} 