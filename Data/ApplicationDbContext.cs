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
    public DbSet<BalancePoint> BalancePoints { get; set; }

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

        // Seed Tables
        modelBuilder.Entity<Table>().HasData(
            new Table { Id = 1, TableNumber = 1, Capacity = 4, Status = "Available" },
            new Table { Id = 2, TableNumber = 2, Capacity = 6, Status = "Available" },
            new Table { Id = 3, TableNumber = 3, Capacity = 2, Status = "Available" }
        );

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FullName = "Test User", PasswordHash = "123456", Role = "Customer", Email = "test@example.com" }
        );
    }
}


