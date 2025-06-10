using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;

public interface IUserService
{
    Task<PagedResult<User>> GetPagedUsersAsync(int page, int pageSize);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> RegisterAsync(UserRegisterRequest request);
    //Task<bool> LoginAsync(string email, string password, string role);
    Task<bool> SendVerificationCodeAsync(string email);
    Task<LoginResponse?> VerifyCodeAndLoginAsync(string email, string code);
}
