using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;  

namespace PRN222_Restaurant.Pages.Admin
{
    public class FeedbackModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackModel(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public List<Feedback> Feedbacks { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 5;

        public async Task OnGetAsync(int? pageNumber)
        {
            int currentPage = pageNumber.GetValueOrDefault(1);
            currentPage = currentPage < 1 ? 1 : currentPage;

            var (feedbacks, totalCount) = await _feedbackService.GetPagedAsync(CurrentPage, PageSize);

            Feedbacks = feedbacks.ToList(); 
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, int currentPage)
        {
            await _feedbackService.DeleteAsync(id);
            return RedirectToPage(new { pageNumber = currentPage }); // giữ lại trang hiện tại
        }
    }
}
