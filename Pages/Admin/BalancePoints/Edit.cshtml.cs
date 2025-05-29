using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using System.ComponentModel.DataAnnotations;

namespace PRN222_Restaurant.Pages.Admin.BalancePoints
{
    public class EditModel : PageModel
    {
        private readonly IBalancePointService _balancePointService;
        private readonly IUserService _userService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(IBalancePointService balancePointService, IUserService userService, ILogger<EditModel> logger)
        {
            _balancePointService = balancePointService;
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public BalancePoint BalancePoint { get; set; } = new();

        public new User? User { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var balancePoint = await _balancePointService.GetByIdAsync(id);

                if (balancePoint == null)
                {
                    _logger.LogWarning("Balance point with ID {Id} not found", id);
                    return NotFound();
                }

                BalancePoint = balancePoint;

                // Get the associated user
                var users = await _userService.GetAllUsersAsync();
                User = users.FirstOrDefault(u => u.Id == BalancePoint.UserId);

                if (User == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found for balance point {BalancePointId}",
                        BalancePoint.UserId, BalancePoint.Id);
                }

                _logger.LogInformation("Successfully loaded balance point {Id} for editing", id);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading balance point for editing with ID {Id}", id);
                ErrorMessage = "An error occurred while loading balance point details. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload user information if validation fails
                try
                {
                    var users = await _userService.GetAllUsersAsync();
                    User = users.FirstOrDefault(u => u.Id == BalancePoint.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while reloading user information");
                }
                return Page();
            }

            try
            {
                // Validate that the balance point exists
                var existingBalancePoint = await _balancePointService.GetByIdAsync(BalancePoint.Id);
                if (existingBalancePoint == null)
                {
                    ErrorMessage = "Balance point not found.";
                    return RedirectToPage("/Admin/BalancePoints/Index");
                }

                // Validate point amount
                if (BalancePoint.Point < 0)
                {
                    ModelState.AddModelError("BalancePoint.Point", "Points cannot be negative.");
                    var users = await _userService.GetAllUsersAsync();
                    User = users.FirstOrDefault(u => u.Id == BalancePoint.UserId);
                    return Page();
                }

                if (BalancePoint.Point > 999999.99m)
                {
                    ModelState.AddModelError("BalancePoint.Point", "Points cannot exceed $999,999.99.");
                    var users = await _userService.GetAllUsersAsync();
                    User = users.FirstOrDefault(u => u.Id == BalancePoint.UserId);
                    return Page();
                }

                await _balancePointService.UpdateAsync(BalancePoint);

                // Get user info for the success message
                var allUsers = await _userService.GetAllUsersAsync();
                var user = allUsers.FirstOrDefault(u => u.Id == BalancePoint.UserId);
                var userName = user?.FullName ?? "Unknown User";

                SuccessMessage = $"Balance points for {userName} have been successfully updated to {BalancePoint.Point:C}.";
                _logger.LogInformation("Balance point {Id} updated successfully. New amount: {Amount}",
                    BalancePoint.Id, BalancePoint.Point);

                return RedirectToPage("/Admin/BalancePoints/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating balance point {Id}", BalancePoint.Id);
                ErrorMessage = "An error occurred while updating the balance point. Please try again.";

                // Reload user information
                try
                {
                    var users = await _userService.GetAllUsersAsync();
                    User = users.FirstOrDefault(u => u.Id == BalancePoint.UserId);
                }
                catch (Exception userEx)
                {
                    _logger.LogError(userEx, "Error occurred while reloading user information");
                }

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
