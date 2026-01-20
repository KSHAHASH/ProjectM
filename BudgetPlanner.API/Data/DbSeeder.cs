using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BudgetPlanner.API.Data
{
    /// <summary>
    /// Database seeder to create initial demo data.
    /// This ensures the database has a demo user for testing.
    /// </summary>
    public static class DbSeeder
    {
        public static void SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Ensure database is created (applies any pending migrations)
            context.Database.Migrate();
            
            // Check if we already have users
            if (context.Users.Any())
            {
                return; // Database already seeded
            }
            
            // Create a demo user with hashed password
            var demoUser = new User
            {
                Id = 1,
                Name = "Demo User",
                Email = "demo@budgetplanner.com",
                MonthlyIncome = 5000m,
                CreatedAt = DateTime.UtcNow
            };
            
            // Hash password "demo123" for demo user
            var passwordHasher = new PasswordHasher<User>();
            demoUser.PasswordHash = passwordHasher.HashPassword(demoUser, "demo123");
            
            context.Users.Add(demoUser);
            context.SaveChanges();
            
           

            //seed Income data for multiple months
            var incomes = new List<Income>
            {
                new Income { UserId = demoUser.Id, Amount = 5000m, Date = new DateTime(2026, 1, 11) },
                new Income { UserId = demoUser.Id, Amount = 6000m, Date = new DateTime(2026, 2, 15) },
                new Income { UserId = demoUser.Id, Amount = 7000m, Date = new DateTime(2026, 3, 16) },
            };
            context.Incomes.AddRange(incomes);
            context.SaveChanges();

             Console.WriteLine("✓ Database seeded with demo user");
            Console.WriteLine("  Email: demo@budgetplanner.com");
            Console.WriteLine("  Password: demo123");
        }
    }
}
