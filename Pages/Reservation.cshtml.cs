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

            // Store reservation details in TempData
            TempData["ReservationDate"] = ReservationDate.ToString("yyyy-MM-dd");
            TempData["ReservationTime"] = ReservationTime.ToString();
            TempData["TableId"] = TableId.ToString();
            TempData["NumberOfGuests"] = NumberOfGuests.ToString();
            TempData["Note"] = Note;

            // If menu items were selected, store them in TempData
            if (!string.IsNullOrEmpty(SelectedItems))
            {
                TempData["SelectedItems"] = SelectedItems;
                return RedirectToPage("/PreOrderConfirmation");
            }

            // If no menu items were selected, redirect to menu selection page
            return RedirectToPage("/PreOrderMenu");
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
            
            // Convert to view model with availability info
            Tables = allTables.Select(t => new TableViewModel
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                IsAvailable = t.Status == "Available"
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
            return JsonSerializer.Serialize(MenuItems, _jsonOptions);
        }
    }

    public class TableViewModel
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
    }
}
