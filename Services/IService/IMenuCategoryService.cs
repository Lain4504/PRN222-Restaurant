using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services.IService
{
    public interface IMenuCategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}
