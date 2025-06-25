using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PRN222_Restaurant.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ITableService _tableService;
        private readonly ApplicationDbContext _context;
        private readonly JsonSerializerOptions _jsonOptions;

        public ReservationModel(IOrderService orderService, ITableService tableService, ApplicationDbContext context)
        {
            _orderService = orderService;
            _tableService = tableService;
            _context = context;
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 64
            };
        }

        [BindProperty]
        [Required(ErrorMessage = "Please select a date")]
        public DateTime ReservationDate { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "Please select a time")]
        public TimeSpan ReservationTime { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter number of guests")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between 1 and 20")]
        public int NumberOfGuests { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a table")]
        public int TableId { get; set; }

        [BindProperty]
        public string? Note { get; set; }

        [BindProperty]
        public string? SelectedItems { get; set; }

        public List<TableViewModel> Tables { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<MenuItem> MenuItems { get; set; } = new();
        
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAvailableTables();
            await LoadMenuData();
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableTables();
                await LoadMenuData();
                return Page();
            }

            // Check if table is available
            if (!await _orderService.IsTableAvailableAsync(TableId, ReservationDate, ReservationTime))
            {
                ModelState.AddModelError("", "Selected table is not available at the chosen time.");
                await LoadAvailableTables();
                await LoadMenuData();
                return Page();
            }

            try
            {
                // Create the order directly here
                var order = new Order
                {
                    UserId = User.Identity.IsAuthenticated ? int.Parse(User.FindFirst("UserId")?.Value ?? "1") : 1,
                    TableId = TableId,
                    OrderDate = DateTime.Now,
                    ReservationTime = ReservationDate.Date.Add(ReservationTime),
                    NumberOfGuests = NumberOfGuests,
                    Note = Note,
                    OrderType = "PreOrder",
                    Status = "Pending",
                    OrderItems = new List<OrderItem>()
                };

                // Parse selected items and add to order
                Console.WriteLine($"SelectedItems received: '{SelectedItems}'");

                if (!string.IsNullOrEmpty(SelectedItems) && SelectedItems != "{}")
                {
                    try
                    {
                        var selectedItemsDict = JsonSerializer.Deserialize<Dictionary<int, int>>(SelectedItems);
                        Console.WriteLine($"Parsed selectedItemsDict: {selectedItemsDict?.Count ?? 0} items");

                        if (selectedItemsDict != null)
                        {
                            foreach (var item in selectedItemsDict)
                            {
                                Console.WriteLine($"Processing item: MenuItemId={item.Key}, Quantity={item.Value}");
                                var menuItem = await _context.MenuItems.FindAsync(item.Key);
                                if (menuItem != null && item.Value > 0)
                                {
                                    order.OrderItems.Add(new OrderItem
                                    {
                                        MenuItemId = item.Key,
                                        Quantity = item.Value,
                                        UnitPrice = menuItem.Price
                                    });
                                    Console.WriteLine($"Added OrderItem: {menuItem.Name} x {item.Value}");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing selected items: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("No selected items found or SelectedItems is empty/null");
                }

                // Save order to database first
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Calculate and update total price after OrderItems are saved
                order.TotalPrice = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
                Console.WriteLine($"Calculated TotalPrice: {order.TotalPrice}");
                Console.WriteLine($"OrderItems count: {order.OrderItems.Count}");

                // Update the order with calculated total price
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Store OrderId in TempData and Session
                TempData["OrderId"] = order.Id.ToString();
                HttpContext.Session.SetString("OrderId", order.Id.ToString());

                // Redirect to PreOrderMenu to show the created order
                return RedirectToPage("/preordermenu");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");
                await LoadAvailableTables();
                await LoadMenuData();
                return Page();
            }
        }

        private async Task LoadAvailableTables()
        {
            // Get all tables from the database
            var allTables = await _context.Tables.ToListAsync();

            // For testing purposes, create 16 tables if there are not enough
            if (allTables.Count < 16)
            {
                int existingCount = allTables.Count;
                for (int i = existingCount + 1; i <= 16; i++)
                {
                    var table = new Table
                    {
                        TableNumber = i,
                        Capacity = (i % 3) + 2, // Randomly assigns 2-4 capacity
                        Status = i % 4 == 0 ? "Occupied" : "Available" // Make every 4th table occupied
                    };
                    _context.Tables.Add(table);
                }
                await _context.SaveChangesAsync();

                // Reload tables after adding new ones
                allTables = await _context.Tables.ToListAsync();
            }

            // Convert to view model with availability info (không còn unavailable)
            Tables = allTables.Select(t => new TableViewModel
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                IsAvailable = t.Status == "Available" && t.Capacity >= NumberOfGuests
            }).ToList();
        }

        private async Task LoadMenuData()
        {
            Categories = await _context.Categories.ToListAsync();
            
            // Load menu items without including the Category navigation property
            MenuItems = await _context.MenuItems
                .Where(m => m.Status == MenuItemStatus.Available)
                .Select(m => new MenuItem
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    ImageUrl = m.ImageUrl,
                    CategoryId = m.CategoryId,
                    Status = m.Status
                })
                .ToListAsync();
        }
        
        // Serializes menu items without circular references
        public string SerializeMenuItems()
        {
            try
            {
                var simplifiedItems = MenuItems.Select(m => new
                {
                    Id = m.Id,
                    Name = m.Name,
                    Price = m.Price,
                    Description = m.Description,
                    CategoryId = m.CategoryId,
                    CategoryName = m.Category?.Name
                }).ToList();

                // Use simple JsonSerializer options without ReferenceHandler
                var simpleOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };

                var json = JsonSerializer.Serialize(simplifiedItems, simpleOptions);
                Console.WriteLine($"SerializeMenuItems result: {json}");
                return json;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serializing menu items: {ex.Message}");
                return "[]";
            }
        }
    }

    public class TableViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        // Xóa IsUnavailable
    }
}
