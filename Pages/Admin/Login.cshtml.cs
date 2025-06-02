using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ErrorMessage { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            public string? Code { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "sendCode")
            {
                return await OnPostSendCodeAsync();
            }
            else if (action == "login")
            {
                return await OnPostLoginAsync();
            }

            return Page();
        }

        private async Task<IActionResult> OnPostSendCodeAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please enter a valid email.";
                return Page();
            }

            var sent = await _userService.SendVerificationCodeAsync(Input.Email);
            if (sent)
            {
                Message = "Verification code sent to your email. Please check your inbox.";
            }
            else
            {
                ErrorMessage = "Failed to send verification code. Try again.";
            }
            return Page();
        }

        private async Task<IActionResult> OnPostLoginAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please enter a valid email.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Code))
            {
                ErrorMessage = "Please enter the verification code.";
                return Page();
            }

            var loginResult = await _userService.VerifyCodeAndLoginAsync(Input.Email, Input.Code);
            if (loginResult == null)
            {
                ErrorMessage = "Invalid or expired verification code.";
                return Page();
            }

            // Lưu token vào session
            HttpContext.Session.SetString("AuthToken", loginResult.Token);

            // Điều hướng dựa trên vai trò
            switch (loginResult.Role)
            {
                case "Admin":
                    return RedirectToPage("/Admin/Dashboard");
                case "Customer":
                    return RedirectToPage("/Index");
                default:
                    return RedirectToPage("/Index");
            }
        }



    }
}
