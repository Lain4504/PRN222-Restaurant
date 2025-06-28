using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using System.Security.Claims;

namespace PRN222_Restaurant.Pages;

[Authorize]
public class PointsHistoryModel : PageModel
{
    private readonly IPointsService _pointsService;

    public PointsHistoryModel(IPointsService pointsService)
    {
        _pointsService = pointsService;
    }

    public int CurrentPoints { get; set; }
    public IEnumerable<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
    public PointsConfig Config { get; set; } = new PointsConfig();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToPage("/Account/Login");
        }

        CurrentPoints = await _pointsService.GetUserPointsAsync(userId.Value);
        PointTransactions = await _pointsService.GetUserPointHistoryAsync(userId.Value, 1, 50);
        Config = _pointsService.GetPointsConfig();

        return Page();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }
}
