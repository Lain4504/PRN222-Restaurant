using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services.IService;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages
{
    public class NotificationsModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public NotificationsModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Admin/Login");
            }

            return Page();
        }

        // Test method - commented out for production
        /*
        public async Task<IActionResult> OnPostCreateTestNotificationsAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Admin/Login");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                // Create test notifications
                for (int i = 1; i <= 15; i++)
                {
                    await _notificationService.AddAsync(new Models.Notification
                    {
                        UserId = userId,
                        Title = $"Test Notification {i}",
                        Message = $"This is test notification number {i} for testing pagination and mark as read functionality.",
                        Type = i % 4 == 0 ? "Success" : i % 3 == 0 ? "Warning" : i % 2 == 0 ? "Error" : "Info",
                        CreatedAt = DateTime.Now.AddMinutes(-i * 5),
                        IsRead = i % 5 == 0, // Some notifications are already read
                        RelatedUrl = i % 3 == 0 ? "/order-list" : "#"
                    });
                }
            }

            return RedirectToPage();
        }
        */
    }
}
