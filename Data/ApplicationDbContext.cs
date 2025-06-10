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
        // Seed Tables
        modelBuilder.Entity<Table>().HasData(
            new Table { Id = 1, TableNumber = 1, Capacity = 4, Status = "Available" },
            new Table { Id = 2, TableNumber = 2, Capacity = 6, Status = "Available" },
            new Table { Id = 3, TableNumber = 3, Capacity = 2, Status = "Available" }
        );
    }
}