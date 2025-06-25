using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services;

namespace PRN222_Restaurant.Pages
{
    public class FeedbackFullModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackFullModel(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 5;

        public PagedResult<Feedback> FeedbackResult { get; set; }

        public async Task OnGetAsync()
        {
            FeedbackResult = await _feedbackService.GetPagedFeedbacksAsync(CurrentPage, PageSize);
        }
    }
}
