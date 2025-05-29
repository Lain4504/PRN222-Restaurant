using PRN222_Restaurant.Models;

public interface IBalancePointService
{
    Task<IEnumerable<BalancePoint>> GetAllAsync();
    Task<BalancePoint?> GetByIdAsync(int id);
    Task UpdateAsync(BalancePoint balancePoint);
    Task Reset(int id);
}
