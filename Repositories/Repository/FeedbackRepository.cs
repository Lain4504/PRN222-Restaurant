using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public FeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Feedback>> GetAllAsync()
    {
        return await _context.Feedbacks.Include(f => f.User).ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _context.Feedbacks.Include(f => f.User).FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task AddAsync(Feedback feedback)
    {
        await _context.Feedbacks.AddAsync(feedback);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Feedback feedback)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback != null)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<(IEnumerable<Feedback>, int)> GetPagedAsync(int pageNumber, int pageSize)
    {
        int validPageNumber = pageNumber < 1 ? 1 : pageNumber;
        int offset = (validPageNumber - 1) * pageSize;

        var items = await _context.Feedbacks.Include(f => f.User)
            .OrderBy(f => f.Id)  
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync();

        int totalCount = await _context.Feedbacks.CountAsync();

        return (items, totalCount);
    }

}
