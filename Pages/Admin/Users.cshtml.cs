using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace PRN222_Restaurant.Pages.Admin
{
    public class UsersModel : PageModel
    {
        [TempData]
        public string? SuccessMessage { get; set; }

        public List<User> Users { get; set; } = new List<User>();
        public int TotalUsers { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (TotalUsers + PageSize - 1) / PageSize;

        public void OnGet(int page = 1)
        {
            CurrentPage = page < 1 ? 1 : page;

            // Mock data - in a real application, this would come from your database
            var allUsers = GetMockUsers();
            TotalUsers = allUsers.Count;

            // Simple pagination logic
            Users = allUsers
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        public IActionResult OnPostDelete(int userId)
        {
            // Mock deletion - in a real application, you would delete from your database
            // For demo purposes, we'll just show a success message
            SuccessMessage = $"Người dùng ID {userId} đã được xóa thành công";
            return RedirectToPage();
        }

        private List<User> GetMockUsers()
        {
            // Generate 25 mock users for demonstration
            var roles = new[] { "Admin", "Manager", "Staff", "User" };
            var users = new List<User>();

            for (int i = 1; i <= 25; i++)
            {
                users.Add(new User
                {
                    Id = i,
                    Name = $"Người dùng {i}",
                    Email = $"user{i}@example.com",
                    Role = roles[i % roles.Length],
                    IsActive = i % 5 != 0, // Every 5th user is inactive
                    AvatarUrl = "" // Using UI Avatars API in the view
                });
            }

            return users;
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
