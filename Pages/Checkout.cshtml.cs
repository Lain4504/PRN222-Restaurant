using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Models;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly IVNPayService _vnPayService;

        public CheckoutModel(IVNPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [BindProperty]
        public PaymentInformation PaymentInfo { get; set; }

        public void OnGet()
        {
            // This page mostly works with client-side data from localStorage
            // In a real application, we might want to load user information if they're logged in
        }

        public IActionResult OnPost()
        {
            var paymentUrl = _vnPayService.CreatePaymentUrl(PaymentInfo, HttpContext);
            return Redirect(paymentUrl);
        }

        public IActionResult OnGetPaymentCallback()
        {
            var response = _vnPayService.ProcessPaymentCallback(Request.Query);
            
            if (response.Success)
            {
                // Payment successful
                TempData["PaymentSuccess"] = true;
                TempData["PaymentMessage"] = "Thanh toán thành công!";
                TempData["TransactionId"] = response.TransactionId;
                return RedirectToPage("/PaymentSuccess");
            }
            else
            {
                // Payment failed
                TempData["PaymentSuccess"] = false;
                TempData["PaymentMessage"] = "Thanh toán thất bại!";
                TempData["PaymentError"] = response.VnPayResponseCode;
                return RedirectToPage("/PaymentFailed");
            }
        }
    }
}
