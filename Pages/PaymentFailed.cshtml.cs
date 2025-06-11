using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class PaymentFailedModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PaymentFailedModel(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public Order? Order { get; set; }
        public int TableNumber { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the order ID from session
            var orderId = HttpContext.Session.GetInt32("CurrentOrderId");
            if (orderId.HasValue)
            {
                // Load the order with related data
                Order = await _context.Orders.FindAsync(orderId.Value);
                
                if (Order != null && Order.TableId.HasValue)
                {
                    // Get table number
                    var table = await _context.Tables.FindAsync(Order.TableId.Value);
                    TableNumber = table?.TableNumber ?? 0;
                }
            }
            
            return Page();
        }
    }
} 