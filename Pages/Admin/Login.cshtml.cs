using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Services.IService;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PRN222_Restaurant.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IReservationSessionService _reservationSessionService;

        public LoginModel(IUserService userService, IReservationSessionService reservationSessionService)
        {
            _userService = userService;
            _reservationSessionService = reservationSessionService;
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


            var user = await _userService.GetUserByEmailAsync(Input.Email);
            if (user == null)
            {
                // Tạo user mới ngầm
                user = new User
                {
                    Email = Input.Email,
                    FullName = "",
                    Role = "Customer",
                    IsActive = true
                };
                var created = await _userService.CreateUserAsync(user);
                if (!created)
                {
                    ErrorMessage = "Failed to create user account. Please try again.";
                    return Page();
                }
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

            // Check if there's saved reservation data for both Admin and Customer
            var savedReservationData = _reservationSessionService.GetReservationData();
            if (savedReservationData != null)
            {
                // Redirect to PreOrderMenu for both Admin and Customer with saved reservation data
                return RedirectToPage("/PreOrderMenu");
            }

            // No reservation data, điều hướng dựa trên vai trò
            switch (loginResult.Role)
            {
                case "Admin":
                    return RedirectToPage("/admin/users");
                case "Customer":
                    return RedirectToPage("/Index");
                default:
                    return RedirectToPage("/admin/users");
            }
        }



    }
}
