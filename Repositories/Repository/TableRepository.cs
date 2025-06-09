using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;

public class TableRepository : ITableRepository
{
    private readonly ApplicationDbContext _context;

    public TableRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Table>> GetAllAsync() => await _context.Tables.ToListAsync();

    public async Task<Table?> GetByIdAsync(int id) => await _context.Tables.FindAsync(id);

    public async Task AddAsync(Table table)
    {
        _context.Tables.Add(table);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Table table)
    {
        var existingTable = await _context.Tables.FindAsync(table.Id);
        if (existingTable == null) return;

        _context.Entry(existingTable).CurrentValues.SetValues(table);
        _context.Entry(existingTable).Property(t => t.Status).IsModified = false;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var table = await _context.Tables.FindAsync(id);
        if (table != null)
        {
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AssignAsync(int id)
    {
        
        var table = await _context.Tables.FindAsync(id);
        if (table != null)
        {
            table.Status = "Reserved";
            await _context.SaveChangesAsync();
        }
    }
    public async Task ChangeStatusAsync(int id, string newStatus)
    {
        
        var table = await _context.Tables.FindAsync(id);
        if (table != null)
        {
            table.Status = newStatus;
            await _context.SaveChangesAsync();
        }
    } 

    public async Task<(IEnumerable<Table>, int)> GetPagedAsync(int pageNumber, int pageSize)
    {
        int validPageNumber = pageNumber < 1 ? 1 : pageNumber;
        int offset = (validPageNumber - 1) * pageSize;
        var items = await _context.Tables
            .OrderBy(t => t.Id)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync();
        int totalCount = await _context.Tables.CountAsync();
        return (items, totalCount);
    }
}
