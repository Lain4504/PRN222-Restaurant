using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _repository;

    public FeedbackService(IFeedbackRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Feedback>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Feedback?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task AddAsync(Feedback feedback) => _repository.AddAsync(feedback);

    public Task UpdateAsync(Feedback feedback) => _repository.UpdateAsync(feedback);

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

    public Task<(IEnumerable<Feedback> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        => _repository.GetPagedAsync(pageNumber, pageSize);
}
