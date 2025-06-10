using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly IOrderService _orderService;

        public ReservationModel(IOrderService orderService)
        {
            _orderService = orderService;
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

        public SelectList AvailableTables { get; set; }
        
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAvailableTables();
            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableTables();
                return Page();
            }
            
            // Check if table is available
            if (!await _orderService.IsTableAvailableAsync(TableId, ReservationDate, ReservationTime))
            {
                ModelState.AddModelError("", "Selected table is not available at the chosen time.");
                await LoadAvailableTables();
                return Page();
            }

            // Store reservation details in TempData
            TempData["ReservationDate"] = ReservationDate.ToString("yyyy-MM-dd");
            TempData["ReservationTime"] = ReservationTime.ToString();
            TempData["TableId"] = TableId.ToString();
            TempData["NumberOfGuests"] = NumberOfGuests.ToString();
            TempData["Note"] = Note;

            return RedirectToPage("/PreOrderMenu");
        }

        private async Task LoadAvailableTables()
        {
            var tables = await _orderService.GetAvailableTablesAsync();
            AvailableTables = new SelectList(tables, "Id", "TableNumber");
        }
    }
}
