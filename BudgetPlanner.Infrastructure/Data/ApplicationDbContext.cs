using BudgetPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    //Table mappings
        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<BudgetRule> BudgetRules { get; set; }
        public DbSet<Goal> Goals { get; set; }
        
        // public DbSet<FinancialRecordExpense> FinancialRecordExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //run the parent DBContext configuration first
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.HasIndex(e => e.Email)
                    .IsUnique();
                entity.Property(e => e.MonthlyIncome)
                    .HasColumnType("decimal(18,2)");
            });

            // Configure Expense entity
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date)
                    .IsRequired();
                entity.Property(e => e.Category)
                    .IsRequired();
                entity.Property(e => e.Type)
                    .IsRequired();
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BudgetRule entity
            modelBuilder.Entity<BudgetRule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MonthlyLimit)
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category)
                    .IsRequired();
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.UserId, e.Category })
                    .IsUnique();
            });

            // Configure Goal entity
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.TargetAmount)
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentSaved)
                    .HasColumnType("decimal(18,2)");
                entity.Property(e => e.Deadline)
                    .IsRequired();
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure FinancialRecordExpense entity
            // modelBuilder.Entity<FinancialRecordExpense>(entity =>
            // {
            //     entity.HasKey(e => e.Id);
            //     entity.Property(e => e.Amount)
            //         .HasColumnType("decimal(18,2)");
            //     entity.Property(e => e.Category)
            //         .IsRequired();
            //     entity.Property(e => e.Type)
            //         .IsRequired();
            //     entity.Property(e => e.Description)
            //         .HasMaxLength(500);
                
            //     // Relationship with FinancialRecord
            //     entity.HasOne(e => e.FinancialRecord)
            //         .WithMany(fr => fr.Expenses)
            //         .HasForeignKey(e => e.FinancialRecordId)
            //         .OnDelete(DeleteBehavior.Cascade);
            // });
        }
    }
}
