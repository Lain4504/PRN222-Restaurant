namespace PRN222_Restaurant.Repositories.IRepository
{
    using PRN222_Restaurant.Models;
    using PRN222_Restaurant.Models.Response;

    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(int id);
        Task AddAsync(Feedback feedback);
        Task UpdateAsync(Feedback feedback);
        Task DeleteAsync(int id);

        Task<PagedResult<Feedback>> GetPagedAsync(int page, int pageSize);

    }
}
