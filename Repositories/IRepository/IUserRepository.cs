using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;

public interface IUserRepository
{
    Task<PagedResult<User>> GetPagedAsync(int page, int pageSize);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}
