using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;

public interface IFeedbackService
{
    Task<IEnumerable<Feedback>> GetAllAsync();
    Task<Feedback?> GetByIdAsync(int id);
    Task AddAsync(Feedback feedback);
    Task UpdateAsync(Feedback feedback);
    Task DeleteAsync(int id);
    Task<PagedResult<Feedback>> GetPagedFeedbacksAsync(int page, int pageSize);

}