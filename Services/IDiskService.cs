using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services;

public interface IDiskService
{
    Task<List<Disk>> GetAllDisksAsync();
    Task<Disk?> GetDiskByIdAsync(int id);
    Task<Disk> CreateDiskAsync(Disk disk);
    Task<Disk?> UpdateDiskAsync(Disk disk);
    Task<bool> DeleteDiskAsync(int id);
} 