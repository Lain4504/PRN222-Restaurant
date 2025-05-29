using System.Data;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;
using PRN222_Restaurant.Repositories.IRepository;

public class BalancePointRepository : IBalancePointRepository
{
    private readonly ApplicationDbContext _context;

    public BalancePointRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<BalancePoint>> GetAllAsync()
    {
        return await _context.BalancePoints.ToListAsync();
    }
    public async Task<BalancePoint?> GetByIdAsync(int id)
    {
        return await _context.BalancePoints.FindAsync(id);
    }
    public async Task UpdateAsync(BalancePoint balancePoint)
    {
        _context.BalancePoints.Update(balancePoint);
        await _context.SaveChangesAsync();
    }
    public async Task Reset(int id)
    {
        var balancePoint = await _context.BalancePoints.FindAsync(id);
        balancePoint.Point = 0;
        await _context.SaveChangesAsync();
    }
}


