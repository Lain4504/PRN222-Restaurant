using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Services.Service
{
    public class MenuItemService : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository;

        public MenuItemService(IMenuItemRepository menuItemRepository)
        {
            _menuItemRepository = menuItemRepository;
        }
        
        public Task<IEnumerable<MenuItem>> GetAllAsync()
        => _menuItemRepository.GetAllAsync();

        public Task<MenuItem?> GetByIdAsync(int id)
            => _menuItemRepository.GetByIdAsync(id);

        public Task<MenuItem> AddAsync(MenuItem menuItem)
            => _menuItemRepository.AddAsync(menuItem);

        public Task UpdateAsync(MenuItem menuItem)
            => _menuItemRepository.UpdateAsync(menuItem);

        public Task DeleteAsync(int id)
            => _menuItemRepository.DeleteAsync(id);
    }
}
