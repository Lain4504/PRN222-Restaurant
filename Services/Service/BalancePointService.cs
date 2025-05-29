using System.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

public class BalancePointService : IBalancePointService
{
    private readonly IBalancePointRepository _repository;

    public BalancePointService(IBalancePointRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<BalancePoint>> GetAllAsync() => _repository.GetAllAsync();

    public Task<BalancePoint?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task UpdateAsync(BalancePoint balancePoint) => _repository.UpdateAsync(balancePoint);

    public Task Reset(int id) => _repository.Reset(id);
}
