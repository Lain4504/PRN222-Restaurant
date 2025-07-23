using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Services.IService;
using PRN222_Restaurant.Services;
using PRN222_Restaurant.Helper;

namespace PRN222_Restaurant.Pages.Admin
{
    public class CustomerPointsModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IPointsService _pointsService;

        public CustomerPointsModel(IUserService userService, IPointsService pointsService)
        {
            _userService = userService;
            _pointsService = pointsService;
        }

        public List<CustomerPointsInfo> Customers { get; set; } = new List<CustomerPointsInfo>();
        public List<PointTransaction> SelectedCustomerTransactions { get; set; } = new List<PointTransaction>();
        public User? SelectedCustomer { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedCustomerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 20;

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if current user is Admin
            if (!AuthHelper.IsAdmin(HttpContext.User))
            {
                return Redirect("/admin/login");
            }

            // Get all users with customer role and their points
            var allUsers = await _userService.GetPagedUsersAsync(1, 1000); // Get all users for now
            
            foreach (var user in allUsers.Items.Where(u => u.Role == "Customer" || u.Role == "User"))
            {
                var pointsBalance = await _pointsService.GetUserPointsAsync(user.Id);
                var recentTransactions = await _pointsService.GetUserPointHistoryAsync(user.Id, 1, 5);
                
                Customers.Add(new CustomerPointsInfo
                {
                    User = user,
                    CurrentPoints = pointsBalance,
                    RecentTransactions = recentTransactions.ToList()
                });
            }

            // Sort by points descending
            Customers = Customers.OrderByDescending(c => c.CurrentPoints).ToList();

            // If a customer is selected, get their detailed transaction history
            if (SelectedCustomerId.HasValue)
            {
                SelectedCustomer = await _userService.GetUserByIdAsync(SelectedCustomerId.Value);
                if (SelectedCustomer != null)
                {
                    SelectedCustomerTransactions = (await _pointsService.GetUserPointHistoryAsync(
                        SelectedCustomerId.Value, CurrentPage, PageSize)).ToList();
                }
            }

            return Page();
        }
    }

    public class CustomerPointsInfo
    {
        public User User { get; set; } = null!;
        public int CurrentPoints { get; set; }
        public List<PointTransaction> RecentTransactions { get; set; } = new List<PointTransaction>();
    }
}
