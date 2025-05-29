namespace PRN222_Restaurant.Repositories.IRepository
{
    using PRN222_Restaurant.Models;

    public interface IBalancePointRepository
    {
        Task<IEnumerable<BalancePoint>> GetAllAsync();
        Task<BalancePoint?> GetByIdAsync(int id);
        Task UpdateAsync(BalancePoint balancePoint);
        Task Reset(int id);
    }
}
    