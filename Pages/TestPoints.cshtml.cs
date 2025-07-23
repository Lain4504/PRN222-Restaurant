using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages
{
    public class TestPointsModel : PageModel
    {
        private readonly IPointsService _pointsService;

        public TestPointsModel(IPointsService pointsService)
        {
            _pointsService = pointsService;
        }

        public int UserId { get; set; }
        public int CurrentPoints { get; set; }
        public IEnumerable<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

        // Debug properties
        public decimal TestOrderAmount { get; set; } = 1250000m; // Test with 1,250,000 VND order
        public int MaxUsablePoints { get; set; }
        public string DebugInfo { get; set; } = "";
        public decimal PointValue { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                DebugInfo = "User not authenticated";
                return Page();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                UserId = userId;
                CurrentPoints = await _pointsService.GetUserPointsAsync(userId);
                PointTransactions = await _pointsService.GetUserPointHistoryAsync(userId, 1, 10);

                // Debug: Calculate max usable points for test order
                MaxUsablePoints = await _pointsService.GetMaxUsablePointsAsync(userId, TestOrderAmount);

                var config = _pointsService.GetPointsConfig();
                PointValue = config.PointValue;
                DebugInfo = $"User ID: {userId}, Current Points: {CurrentPoints}, " +
                           $"Test Order: {TestOrderAmount:N0} VNĐ, Max Usable: {MaxUsablePoints}, " +
                           $"Point Value: {config.PointValue:N0} VNĐ, Max Usage %: {config.MaxPointsUsagePercentage * 100}%, " +
                           $"Points Per VND: {config.PointsPerVND}, Min Order: {config.MinimumOrderAmount:N0} VNĐ";
            }
            else
            {
                DebugInfo = "Could not get user ID from claims";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddPointsAsync(int pointsToAdd)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/admin/login");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                // Create a fake order ID for testing
                var fakeOrderId = new Random().Next(10000, 99999);
                
                // Award points as if it was from an order
                var orderAmount = pointsToAdd * 20; // Simulate order amount (20x points = order value)
                await _pointsService.AwardPointsAsync(userId, fakeOrderId, orderAmount, "Test points");
                
                TempData["Message"] = $"Added {pointsToAdd} test points successfully!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAwardWelcomeBonusAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/admin/login");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                var success = await _pointsService.AwardWelcomeBonusAsync(userId);
                if (success)
                {
                    TempData["Message"] = "Welcome bonus awarded successfully!";
                }
                else
                {
                    TempData["Error"] = "Welcome bonus already awarded or error occurred.";
                }
            }

            return RedirectToPage();
        }
    }
}
