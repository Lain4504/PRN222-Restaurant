using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Món Khai Vị" },
            new Category { Id = 2, Name = "Món Chính" },
            new Category { Id = 3, Name = "Món Tráng Miệng" }
        );

        // Seed MenuItems
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, Name = "Gỏi Cuốn", Description = "Gỏi cuốn tôm thịt", Price = 45000, CategoryId = 1, ImageUrl = "/images/goi-cuon.jpg" },
            new MenuItem { Id = 2, Name = "Phở Bò", Description = "Phở bò truyền thống", Price = 65000, CategoryId = 2, ImageUrl = "/images/pho-bo.jpg" },
            new MenuItem { Id = 3, Name = "Bánh Flan", Description = "Bánh flan trứng sữa", Price = 35000, CategoryId = 3, ImageUrl = "/images/banh-flan.jpg" }
        );

        // Seed 30 tables: 1-10 (2), 11-20 (4), 21-25 (6), 26-30 (8)
        modelBuilder.Entity<Table>().HasData(
            Enumerable.Range(1, 10).Select(i => new Table { Id = i, TableNumber = i, Capacity = 2, Status = "Available" })
            .Concat(Enumerable.Range(11, 10).Select(i => new Table { Id = i, TableNumber = i, Capacity = 4, Status = "Available" }))
            .Concat(Enumerable.Range(21, 5).Select(i => new Table { Id = i, TableNumber = i, Capacity = 6, Status = "Available" }))
            .Concat(Enumerable.Range(26, 5).Select(i => new Table { Id = i, TableNumber = i, Capacity = 8, Status = "Available" }))
            .ToArray()
        );

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Customer",
                PasswordHash = "password", // In a real app, should use password hashing
                IsActive = true
            },
            new User
            {
                Id = 2,
                FullName = "Jane Smith",
                Email = "jane@example.com",
                Role = "Customer",
                PasswordHash = "password", // In a real app, should use password hashing
                IsActive = true
            },
            new User
            {
                Id = 3,
                FullName = "Admin User",
                Email = "admin@example.com",
                Role = "Admin",
                PasswordHash = "admin123", // In a real app, should use password hashing
                IsActive = true
            }
        );

        // Note: We don't seed Orders and OrderItems in OnModelCreating because they have 
        // navigation properties that EF Core can't handle well in HasData.
        // Instead, these are seeded using the SeedData class during application startup.
    }
}