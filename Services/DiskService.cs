using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services;

public class DiskService : IDiskService
{
    private readonly ApplicationDbContext _context;

    public DiskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Disk>> GetAllDisksAsync()
    {
        return await _context.Disks.ToListAsync();
    }

    public async Task<Disk?> GetDiskByIdAsync(int id)
    {
        return await _context.Disks.FindAsync(id);
    }

    public async Task<Disk> CreateDiskAsync(Disk disk)
    {
        _context.Disks.Add(disk);
        await _context.SaveChangesAsync();
        return disk;
    }

    public async Task<Disk?> UpdateDiskAsync(int id, Disk disk)
    {
        var existingDisk = await _context.Disks.FindAsync(id);
        if (existingDisk == null) return null;

        existingDisk.Name = disk.Name;
        existingDisk.Description = disk.Description;
        existingDisk.Price = disk.Price;
        existingDisk.IsAvailable = disk.IsAvailable;

        await _context.SaveChangesAsync();
        return existingDisk;
    }

    public async Task<bool> DeleteDiskAsync(int id)
    {
        var disk = await _context.Disks.FindAsync(id);
        if (disk == null) return false;

        _context.Disks.Remove(disk);
        await _context.SaveChangesAsync();
        return true;
    }
} 