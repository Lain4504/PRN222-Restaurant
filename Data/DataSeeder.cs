using Microsoft.EntityFrameworkCore;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (await context.Users.AnyAsync() || await context.BalancePoints.AnyAsync())
            {
                return; // Data already seeded
            }

            // Seed Users
            var users = new List<User>
            {
                new User
                {
                    FullName = "John Doe",
                    Email = "john.doe@example.com",
                    Role = "Customer",
                    IsActive = true
                },
                new User
                {
                    FullName = "Jane Smith",
                    Email = "jane.smith@example.com",
                    Role = "Customer",
                    IsActive = true
                },
                new User
                {
                    FullName = "Admin User",
                    Email = "admin@restaurant.com",
                    Role = "Admin",
                    IsActive = true
                },
                new User
                {
                    FullName = "Bob Johnson",
                    Email = "bob.johnson@example.com",
                    Role = "Customer",
                    IsActive = true
                },
                new User
                {
                    FullName = "Alice Brown",
                    Email = "alice.brown@example.com",
                    Role = "Customer",
                    IsActive = true
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Seed BalancePoints
            var balancePoints = new List<BalancePoint>
            {
                new BalancePoint
                {
                    Point = 100.50m,
                    UserId = users[0].Id // John Doe
                },
                new BalancePoint
                {
                    Point = 250.75m,
                    UserId = users[1].Id // Jane Smith
                },
                new BalancePoint
                {
                    Point = 0.00m,
                    UserId = users[2].Id // Admin User
                },
                new BalancePoint
                {
                    Point = 75.25m,
                    UserId = users[3].Id // Bob Johnson
                },
                new BalancePoint
                {
                    Point = 500.00m,
                    UserId = users[4].Id // Alice Brown
                }
            };

            await context.BalancePoints.AddRangeAsync(balancePoints);
            await context.SaveChangesAsync();
        }
    }
}
