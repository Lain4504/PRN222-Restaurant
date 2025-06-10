using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Text.Json;
using System.Text.Json.Serialization;
using PRN222_Restaurant.Data;

namespace PRN222_Restaurant.Pages
{
    public class PreOrderMenuModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly JsonSerializerOptions _jsonOptions;

        public PreOrderMenuModel(
            IOrderService orderService,
            ApplicationDbContext context)
        {
            _orderService = orderService;
            _context = context;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 64
            };
        }

        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int TableNumber { get; set; }
        public int NumberOfGuests { get; set; }
        public List<Category> Categories { get; set; }
        public List<MenuItem> MenuItems { get; set; }

        [BindProperty]
        public string SelectedItems { get; set; }

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

            // Get table number
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return RedirectToPage("/Reservation");
            }
            TableNumber = table.TableNumber;

            // Load categories and menu items
            Categories = await _context.Categories.ToListAsync();
            
            // Avoid circular references by explicitly selecting only needed properties
            MenuItems = await _context.MenuItems
                .Select(m => new MenuItem
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl,
                    CategoryId = m.CategoryId,
                    Status = m.Status,
                    Category = new Category { 
                        Id = m.Category.Id, 
                        Name = m.Category.Name 
                    }
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(SelectedItems))
            {
                ModelState.AddModelError("", "Please select at least one item");
                return Page();
            }

            try
            {
                // Parse selected items
                var selectedItems = JsonSerializer.Deserialize<Dictionary<int, int>>(SelectedItems);
                if (selectedItems == null || !selectedItems.Any())
                {
                    ModelState.AddModelError("", "Please select at least one item");
                    return Page();
                }

                // Get reservation details from TempData
                var reservationDate = DateTime.Parse(TempData["ReservationDate"].ToString());
                var reservationTime = TimeSpan.Parse(TempData["ReservationTime"].ToString());
                var tableId = int.Parse(TempData["TableId"].ToString());
                var numberOfGuests = int.Parse(TempData["NumberOfGuests"].ToString());
                var note = TempData["Note"]?.ToString();

                // Create pre-order
                var order = new Order
                {
                    UserId = int.Parse(User.FindFirst("UserId")?.Value), // Get from authenticated user
                    TableId = tableId,
                    ReservationTime = reservationDate.Date.Add(reservationTime),
                    NumberOfGuests = numberOfGuests,
                    Note = note
                };

                // Create pre-order using service
                var createdOrder = await _orderService.CreatePreOrderAsync(order, selectedItems);

                // Clear TempData
                TempData.Clear();

                // Redirect to confirmation page
                return RedirectToPage("/PreOrderConfirmation", new { id = createdOrder.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your order. Please try again.");
                return Page();
            }
        }
        
        // Helper method to serialize menu items without circular references
        public string SerializeMenuItems()
        {
            return JsonSerializer.Serialize(MenuItems, _jsonOptions);
        }
    }
} 