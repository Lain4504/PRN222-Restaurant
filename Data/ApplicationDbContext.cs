using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Disk> Disks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed data
        modelBuilder.Entity<Disk>().HasData(
            new Disk
            {
                Id = 1,
                Name = "Pizza Margherita",
                Description = "Classic Italian pizza with tomato sauce and mozzarella",
                Price = 12.99m,
                IsAvailable = true
            },
            new Disk
            {
                Id = 2,
                Name = "Spaghetti Carbonara",
                Description = "Traditional Roman pasta with eggs, cheese, and guanciale",
                Price = 15.99m,
                IsAvailable = true
            }
        );
    }
} 