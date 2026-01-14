using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            
            // Create a demo user
            var demoUser = new User
            {
                Id = 1,
                Name = "Demo User",
                Email = "demo@budgetplanner.com",
                MonthlyIncome = 5000m
            };
            
            context.Users.Add(demoUser);
            context.SaveChanges();
            
            Console.WriteLine("✓ Database seeded with demo user (ID: 1, Email: demo@budgetplanner.com)");
        }
    }
}
