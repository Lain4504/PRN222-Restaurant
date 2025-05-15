using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Data;
using PRN222_Restaurant.Hubs;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services;

public class DiskService : IDiskService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<DiskHub> _hubContext;

    public DiskService(ApplicationDbContext context, IHubContext<DiskHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<List<Disk>> GetAllDisksAsync()
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
        await _hubContext.Clients.All.SendAsync("ReceiveDiskNotification", "New disk added", disk);
        return disk;
    }

    public async Task<Disk?> UpdateDiskAsync(Disk disk)
    {
        var existingDisk = await _context.Disks.FindAsync(disk.Id);
        if (existingDisk == null) return null;

        _context.Entry(existingDisk).CurrentValues.SetValues(disk);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveDiskNotification", "Disk updated", disk);
        return disk;
    }

    public async Task<bool> DeleteDiskAsync(int id)
    {
        var disk = await _context.Disks.FindAsync(id);
        if (disk == null) return false;

        _context.Disks.Remove(disk);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("ReceiveDiskNotification", "Disk deleted", disk);
        return true;
    }
} 