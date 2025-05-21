using PRN222_Restaurant.Models;

public interface IFeedbackService
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(int id);
    Task AddAsync(Feedback feedback);
    Task UpdateAsync(Feedback feedback);
    Task DeleteAsync(int id);
    Task<(IEnumerable<Feedback> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}
