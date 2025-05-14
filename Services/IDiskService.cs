using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Services;

public interface IDiskService
{
    Task<IEnumerable<Disk>> GetAllDisksAsync();
    Task<Disk?> GetDiskByIdAsync(int id);
    Task<Disk> CreateDiskAsync(Disk disk);
    Task<Disk?> UpdateDiskAsync(int id, Disk disk);
    Task<bool> DeleteDiskAsync(int id);
} 