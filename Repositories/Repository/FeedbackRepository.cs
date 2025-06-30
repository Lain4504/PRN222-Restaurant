using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Models.Response;
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
        return await _context.Feedbacks
            .Include(f => f.User)
            .Include(f => f.Order)
            .ToListAsync();
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .Include(f => f.Order)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task AddAsync(Feedback feedback)
    {
        try
        {
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {

            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            throw new Exception("Lỗi khi thêm Feedback: " + innerMessage, dbEx);
        }
    }
    public async Task<PagedResult<Feedback>> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Feedbacks
            .Include(f => f.User)
            .Include(f => f.Order)
            .OrderBy(f => f.Id);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Feedback>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
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


}