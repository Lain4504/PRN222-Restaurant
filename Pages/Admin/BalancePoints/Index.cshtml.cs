using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Pages.Admin.BalancePoints
{
    public class IndexModel : PageModel
    {
        private readonly IBalancePointService _balancePointService;
        private readonly IUserService _userService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IBalancePointService balancePointService, IUserService userService, ILogger<IndexModel> logger)
        {
            _balancePointService = balancePointService;
            _userService = userService;
            _logger = logger;
        }

        public List<BalancePoint> BalancePoints { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                var balancePoints = await _balancePointService.GetAllAsync();
                var users = await _userService.GetAllUsersAsync();

                // Create a dictionary for quick user lookup
                var userDict = users.ToDictionary(u => u.Id, u => u);

                // Populate the User property for each balance point
                BalancePoints = balancePoints.Select(bp => new BalancePoint
                {
                    Id = bp.Id,
                    Point = bp.Point,
                    UserId = bp.UserId,
                    User = userDict.ContainsKey(bp.UserId) ? userDict[bp.UserId] : null
                }).ToList();

                _logger.LogInformation("Successfully loaded {Count} balance points", BalancePoints.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading balance points");
                ErrorMessage = "An error occurred while loading balance points. Please try again.";
                BalancePoints = new List<BalancePoint>();
            }
        }

        public async Task<IActionResult> OnPostResetAsync(int balancePointId)
        {
            try
            {
                var balancePoint = await _balancePointService.GetByIdAsync(balancePointId);
                if (balancePoint == null)
                {
                    ErrorMessage = "Balance point not found.";
                    return RedirectToPage();
                }

                // Get user info for the success message
                var users = await _userService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.Id == balancePoint.UserId);
                var userName = user?.FullName ?? "Unknown User";

                await _balancePointService.Reset(balancePointId);

                SuccessMessage = $"Balance points for {userName} have been successfully reset to zero.";
                _logger.LogInformation("Balance point {Id} reset to zero for user {UserId}", balancePointId, balancePoint.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting balance point {Id}", balancePointId);
                ErrorMessage = "An error occurred while resetting the balance point. Please try again.";
            }

            return RedirectToPage();
        }
    }
}
