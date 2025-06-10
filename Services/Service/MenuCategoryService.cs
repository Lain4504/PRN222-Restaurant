using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;
using PRN222_Restaurant.Repositories.Repository;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class MenuCategoryService : IMenuCategoryService
    {
        private readonly IMenuCategoryRepository _menuCategoryRepository;

        public MenuCategoryService(IMenuCategoryRepository menuCategoryRepository)
        {
            _menuCategoryRepository = menuCategoryRepository;
        }

        public Task<IEnumerable<Category>> GetAllAsync() => _menuCategoryRepository.GetAllAsync();
        
        public Task<Category?> GetByIdAsync(int id) => _menuCategoryRepository.GetByIdAsync(id);
        
        public Task<Category> AddAsync(Category category) => _menuCategoryRepository.AddAsync(category);
        
        public Task UpdateAsync(Category category) => _menuCategoryRepository.UpdateAsync(category);
        
        public Task DeleteAsync(int id) => _menuCategoryRepository.DeleteAsync(id);
    }
}
