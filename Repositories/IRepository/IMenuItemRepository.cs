using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Repositories.IRepository
{
    public interface IMenuItemRepository
    {
        Task<IEnumerable<MenuItem>> GetAllAsync();
        Task<MenuItem?> GetByIdAsync(int id);
        Task<MenuItem> AddAsync(MenuItem menuItem);
        Task UpdateAsync(MenuItem menuItem);
        Task DeleteAsync(int id);
    }
}
