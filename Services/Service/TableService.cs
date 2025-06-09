using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

public class TableService : ITableService
{
    private readonly ITableRepository _repository;

    public TableService(ITableRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Table>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Table?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task AddAsync(Table table) => _repository.AddAsync(table);

    public Task UpdateAsync(Table table) => _repository.UpdateAsync(table);

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    public Task AssignAsync(int id) => _repository.AssignAsync(id);

    public Task ChangeStatusAsync(int id, string newStatus) => _repository.ChangeStatusAsync(id, newStatus);

    public Task<(IEnumerable<Table> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        => _repository.GetPagedAsync(pageNumber, pageSize);
}
