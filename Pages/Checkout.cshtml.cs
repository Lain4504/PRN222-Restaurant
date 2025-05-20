using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Pages
{
    public class CheckoutModel : PageModel
    {
        public void OnGet()
        {
            // This page mostly works with client-side data from localStorage
            // In a real application, we might want to load user information if they're logged in
        }
    }
}
