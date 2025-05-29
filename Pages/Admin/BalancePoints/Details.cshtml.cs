using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Pages.Admin.BalancePoints
{
    public class DetailsModel : PageModel
    {
        private readonly IBalancePointService _balancePointService;
        private readonly IUserService _userService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IBalancePointService balancePointService, IUserService userService, ILogger<DetailsModel> logger)
        {
            _balancePointService = balancePointService;
            _userService = userService;
            _logger = logger;
        }

        public BalancePoint? BalancePoint { get; set; }
        public new User? User { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                BalancePoint = await _balancePointService.GetByIdAsync(id);

                if (BalancePoint == null)
                {
                    _logger.LogWarning("Balance point with ID {Id} not found", id);
                    return NotFound();
                }

                // Get the associated user
                var users = await _userService.GetAllUsersAsync();
                User = users.FirstOrDefault(u => u.Id == BalancePoint.UserId);

                if (User == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for balance point {BalancePointId}",
                        BalancePoint.UserId, BalancePoint.Id);
                }

                _logger.LogInformation("Successfully loaded balance point {Id} for user {UserId}", id, BalancePoint.UserId);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading balance point details for ID {Id}", id);
                ErrorMessage = "An error occurred while loading balance point details. Please try again.";
                return Page();
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
                    return RedirectToPage("/Admin/BalancePoints/Index");
                }

                // Get user info for the success message
                var users = await _userService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.Id == balancePoint.UserId);
                var userName = user?.FullName ?? "Unknown User";

                await _balancePointService.Reset(balancePointId);

                SuccessMessage = $"Balance points for {userName} have been successfully reset to zero.";
                _logger.LogInformation("Balance point {Id} reset to zero for user {UserId}", balancePointId, balancePoint.UserId);

                return RedirectToPage("/Admin/BalancePoints/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting balance point {Id}", balancePointId);
                ErrorMessage = "An error occurred while resetting the balance point. Please try again.";
                return RedirectToPage("/Admin/BalancePoints/Index");
            }
        }
    }
}
