using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IFeedbackService feedbackService, ILogger<IndexModel> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        public List<Feedback> Feedbacks { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allFeedbacks = await _feedbackService.GetAllAsync();

            Feedbacks = allFeedbacks
                .OrderByDescending(f => f.CreatedAt)
                .ToList();
        }

    }

}