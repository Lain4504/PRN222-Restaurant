using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
using PRN222_Restaurant.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class FeedbackModel : PageModel
    {
        private readonly IFeedbackService _feedbackService;
        private const int DefaultPageSize = 10;

        public FeedbackModel(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        public PagedResult<Feedback> FeedbackResult { get; set; } = new();
        public List<Feedback> Feedbacks => FeedbackResult.Items;

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DefaultPageSize;

        public async Task<IActionResult> OnGetAsync()
        {
            if (CurrentPage < 1) CurrentPage = 1;
            if (PageSize < 1) PageSize = DefaultPageSize;

            FeedbackResult = await _feedbackService.GetPagedFeedbacksAsync(CurrentPage, PageSize);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _feedbackService.DeleteAsync(id);
            SuccessMessage = "Xóa phản hồi thành công.";
            return RedirectToPage(new { CurrentPage, PageSize });
        }
    }
}