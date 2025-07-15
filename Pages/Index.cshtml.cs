using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Services.IService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IMenuItemService _menuItemService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IFeedbackService feedbackService, ILogger<IndexModel> logger, IMenuItemService menuItemService)
        {
            _feedbackService = feedbackService;
            _logger = logger;
            _menuItemService = menuItemService;
        }

        public List<Feedback> Feedbacks { get; set; } = new();
        public List<MenuItem> FeaturedDishes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allFeedbacks = await _feedbackService.GetAllAsync();

            Feedbacks = allFeedbacks
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            var allMenuItems = await _menuItemService.GetAllAsync();

            FeaturedDishes = allMenuItems
                .Where(m => m.Status == MenuItemStatus.Available)
                .OrderByDescending(m => m.BuyCount)
                .ThenByDescending(m => m.Price)
                .Take(3) // Lấy top 3 món ăn nổi bật
                .ToList();
        }
    }
}