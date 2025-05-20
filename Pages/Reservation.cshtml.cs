using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Pages
{
    public class ReservationModel : PageModel
    {
        [BindProperty]
        public Reservation ReservationRequest { get; set; } = new Reservation();
        
        public void OnGet()
        {
            // Initialize with default values if needed
            ReservationRequest.ReservationDate = DateTime.Now.Date.AddDays(1);
            ReservationRequest.ReservationTime = new TimeSpan(19, 0, 0); // 7:00 PM default
        }
        
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // In a real application, this would save to database
            // and potentially send confirmation emails
            
            // For now, just return success
            TempData["ReservationSuccess"] = true;
            TempData["ReservationDetails"] = $"Table for {ReservationRequest.PartySize} on {ReservationRequest.ReservationDate.ToShortDateString()} at {ReservationRequest.ReservationTime.ToString(@"hh\:mm")}";
            
            return RedirectToPage();
        }
    }
    
    // This class would normally be in the Models folder
    // It's defined here for simplicity in this example
    public class Reservation
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int PartySize { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public string SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    }
}
