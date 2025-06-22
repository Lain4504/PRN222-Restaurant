using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PRN222_Restaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PRN222_Restaurant.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Check if we already have orders
                if (context.Orders.Any())
                {
                    return;   // DB has been seeded
                }

                // Seed Users if not exists
                if (!context.Users.Any())
                {
                    SeedUsers(context);
                }

                // Seed Orders and OrderItems
                SeedOrders(context);

                context.SaveChanges();
            }
        }

        private static void SeedUsers(ApplicationDbContext context)
        {
            context.Users.AddRange(
                new User
                {
                    Id = 1,
                    FullName = "John Doe",
                    Email = "john@example.com",
                    Role = "Customer",
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    FullName = "Jane Smith",
                    Email = "jane@example.com",
                    Role = "Customer",
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    FullName = "Admin User",
                    Email = "admin@example.com",
                    Role = "Admin",
                    IsActive = true
                }
            );

            context.SaveChanges();
        }

        private static void SeedOrders(ApplicationDbContext context)
        {
            // Create immediate orders
            var immediateOrders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    UserId = 1,
                    TableId = 1,
                    OrderDate = DateTime.Now.AddDays(-2),
                    OrderType = "Immediate",
                    Status = "Completed",
                    TotalPrice = 145000,
                    Note = "Không hành",
                    OrderItems = new List<OrderItem>()
                },
                new Order
                {
                    Id = 2,
                    UserId = 2,
                    TableId = 2,
                    OrderDate = DateTime.Now.AddDays(-1),
                    OrderType = "Immediate",
                    Status = "Completed",
                    TotalPrice = 210000,
                    Note = "",
                    OrderItems = new List<OrderItem>()
                },
                new Order
                {
                    Id = 3,
                    UserId = 1,
                    TableId = 3,
                    OrderDate = DateTime.Now.AddHours(-5),
                    OrderType = "Immediate",
                    Status = "Served",
                    TotalPrice = 100000,
                    Note = "Ít cay",
                    OrderItems = new List<OrderItem>()
                }
            };

            // Create pre-orders (reservations)
            var preOrders = new List<Order>
            {
                new Order
                {
                    Id = 4,
                    UserId = 1,
                    TableId = 1,
                    OrderDate = DateTime.Now.AddDays(-1),
                    ReservationTime = DateTime.Now.AddDays(1).Date.AddHours(18), // Tomorrow 6PM
                    OrderType = "PreOrder",
                    Status = "Pending",
                    TotalPrice = 175000,
                    Note = "Mừng sinh nhật",
                    NumberOfGuests = 4,
                    OrderItems = new List<OrderItem>()
                },
                new Order
                {
                    Id = 5,
                    UserId = 2,
                    TableId = 2,
                    OrderDate = DateTime.Now.AddHours(-12),
                    ReservationTime = DateTime.Now.AddDays(2).Date.AddHours(19), // Day after tomorrow 7PM
                    OrderType = "PreOrder",
                    Status = "Pending",
                    TotalPrice = 260000,
                    Note = "",
                    NumberOfGuests = 6,
                    OrderItems = new List<OrderItem>()
                }
            };

            // Add all orders
            context.Orders.AddRange(immediateOrders);
            context.Orders.AddRange(preOrders);
            context.SaveChanges();

            // Add order items
            var orderItems = new List<OrderItem>
            {
                // Items for Order 1
                new OrderItem
                {
                    Id = 1,
                    OrderId = 1,
                    MenuItemId = 1, // Gỏi Cuốn
                    Quantity = 2,
                    UnitPrice = 45000
                },
                new OrderItem
                {
                    Id = 2,
                    OrderId = 1,
                    MenuItemId = 2, // Phở Bò
                    Quantity = 1,
                    UnitPrice = 65000
                },
                
                // Items for Order 2
                new OrderItem
                {
                    Id = 3,
                    OrderId = 2,
                    MenuItemId = 2, // Phở Bò
                    Quantity = 2,
                    UnitPrice = 65000
                },
                new OrderItem
                {
                    Id = 4,
                    OrderId = 2,
                    MenuItemId = 1, // Gỏi Cuốn
                    Quantity = 1,
                    UnitPrice = 45000
                },
                new OrderItem
                {
                    Id = 5,
                    OrderId = 2,
                    MenuItemId = 3, // Bánh Flan
                    Quantity = 1,
                    UnitPrice = 35000
                },
                
                // Items for Order 3
                new OrderItem
                {
                    Id = 6,
                    OrderId = 3,
                    MenuItemId = 2, // Phở Bò
                    Quantity = 1,
                    UnitPrice = 65000
                },
                new OrderItem
                {
                    Id = 7,
                    OrderId = 3,
                    MenuItemId = 3, // Bánh Flan
                    Quantity = 1,
                    UnitPrice = 35000
                },
                
                // Items for Order 4 (PreOrder)
                new OrderItem
                {
                    Id = 8,
                    OrderId = 4,
                    MenuItemId = 1, // Gỏi Cuốn
                    Quantity = 2,
                    UnitPrice = 45000
                },
                new OrderItem
                {
                    Id = 9,
                    OrderId = 4,
                    MenuItemId = 2, // Phở Bò
                    Quantity = 1,
                    UnitPrice = 65000
                },
                new OrderItem
                {
                    Id = 10,
                    OrderId = 4,
                    MenuItemId = 3, // Bánh Flan
                    Quantity = 1,
                    UnitPrice = 35000
                },
                
                // Items for Order 5 (PreOrder)
                new OrderItem
                {
                    Id = 11,
                    OrderId = 5,
                    MenuItemId = 1, // Gỏi Cuốn
                    Quantity = 2,
                    UnitPrice = 45000
                },
                new OrderItem
                {
                    Id = 12,
                    OrderId = 5,
                    MenuItemId = 2, // Phở Bò
                    Quantity = 2,
                    UnitPrice = 65000
                },
                new OrderItem
                {
                    Id = 13,
                    OrderId = 5,
                    MenuItemId = 3, // Bánh Flan
                    Quantity = 2,
                    UnitPrice = 35000
                }
            };

            context.OrderItems.AddRange(orderItems);
            context.SaveChanges();
        }
    }
}