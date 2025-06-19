using PRN222_Restaurant.Models;

public interface ITableRepository
{
    Task<IEnumerable<Table>> GetAllAsync();
    Task<Table?> GetByIdAsync(int id);
    Task AddAsync(Table table);
    Task UpdateAsync(Table table);
    Task DeleteAsync(int id);
    Task AssignAsync(int id);
    Task ChangeStatusAsync(int id, string newStatus);
    Task<(IEnumerable<Table> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}
