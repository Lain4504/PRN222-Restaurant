using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN222_Restaurant.Pages.Admin.Users
{
    public class EditModel : PageModel
    {
        [BindProperty]
        public User User { get; set; } = new User();

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty] 
        public string? ConfirmPassword { get; set; }

        public bool IsNewUser => User.Id == 0;

        [TempData]
        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet(int? id)
        {
            if (id.HasValue)
            {
                // Edit existing user
                var user = GetUserById(id.Value);
                if (user == null)
                {
                    return NotFound();
                }
                
                User = user;
            }
            else
            {
                // Create new user
                User = new User
                {
                    IsActive = true // Default to active for new users
                };
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            // Validate passwords
            if (IsNewUser && string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Mật khẩu không được để trống cho người dùng mới";
                return Page();
            }

            if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu không khớp";
                return Page();
            }

            // In a real application, you would save to database
            // For demo purposes, we'll just redirect with a success message
            
            if (IsNewUser)
            {
                SuccessMessage = "Đã thêm người dùng mới thành công";
            }            else
            {
                SuccessMessage = $"Đã cập nhật người dùng {User.Name} thành công";
            }

            return RedirectToPage("/admin/users");
        }

        private User? GetUserById(int id)
        {
            // In a real application, you would get this from your database
            // For demo purposes, we'll just return a mock user
            if (id <= 0 || id > 25) return null;

            var roles = new[] { "Admin", "Manager", "Staff", "User" };
            
            return new User
            {
                Id = id,
                Name = $"Người dùng {id}",
                Email = $"user{id}@example.com",
                Role = roles[id % roles.Length],
                IsActive = id % 5 != 0, // Every 5th user is inactive
                AvatarUrl = "" // Using UI Avatars API in the view
            };
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
