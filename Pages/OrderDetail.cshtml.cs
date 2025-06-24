using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_Restaurant.Pages
{
    public class OrderDetailModel : PageModel
    {
        [Microsoft.AspNetCore.Mvc.FromRoute]
        public int OrderId { get; set; }
        public void OnGet() { }
    }
}
