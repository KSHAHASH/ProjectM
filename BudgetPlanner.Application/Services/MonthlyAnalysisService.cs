using BudgetPlanner.Application.DTOs;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Application.Services
{
    public interface IMonthlyAnalysisService
    {
        Task<MonthlyAnalysisDto> GetMonthlyAnalysisAsync(int userId, int year, int month);
    }

    public class MonthlyAnalysisService : IMonthlyAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly IRecommendationsService _recommendationsService;

        public MonthlyAnalysisService(ApplicationDbContext context, IRecommendationsService recommendationsService)
        {
            _context = context;
            _recommendationsService = recommendationsService;
        }

        public async Task<MonthlyAnalysisDto> GetMonthlyAnalysisAsync(int userId, int year, int month)
        {
            var result = new MonthlyAnalysisDto();

            // Get user with their details from the database based on the userID, User Table entries
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                result.HasSufficientData = false;
                return result;
            }

            // Define date ranges
            var currentMonthStart = new DateTime(year, month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);

            // Get current month expenses
            var currentExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= currentMonthStart && e.Date <= currentMonthEnd)
                .ToListAsync();

            // Get previous month expenses for comparison
            var previousExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= previousMonthStart && e.Date <= previousMonthEnd)
                .ToListAsync();
            
            //get current month income
            var currentIncomes = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= currentMonthStart && i.Date <= currentMonthEnd)
                .ToListAsync();

            // get previous month income
            var previousIncomes = await _context.Incomes
                .Where(i => i.UserId == userId && i.Date >= previousMonthStart && i.Date <= previousMonthEnd)
                .ToListAsync();

            // Check if we have sufficient data
            result.HasSufficientData = currentExpenses.Any() && previousExpenses.Any();

            // Calculate totals for current month
            result.TotalIncome = currentIncomes.Sum(i => i.Amount);
            result.TotalExpenses = currentExpenses.Sum(e => e.Amount);
            result.TotalSavings = result.TotalIncome - result.TotalExpenses;
            result.SavingsRate = result.TotalIncome > 0 ? (result.TotalSavings / result.TotalIncome) * 100 : 0;

            // Calculate changes from previous month
            if (result.HasSufficientData)
            {
                var previousTotalIncome = previousIncomes.Sum(i => i.Amount);
                var previousTotalExpenses = previousExpenses.Sum(e => e.Amount);
                var previousSavings = previousTotalIncome - previousTotalExpenses;
                var previousSavingsRate = previousTotalIncome > 0 ? (previousSavings / previousTotalIncome) * 100 : 0;

                // Calculate percentage changes
                result.IncomeChange = previousTotalIncome > 0 
                    ? ((result.TotalIncome - previousTotalIncome) / previousTotalIncome) * 100 
                    : 0;    
                result.ExpenseChange = previousTotalExpenses > 0 
                    ? ((result.TotalExpenses - previousTotalExpenses) / previousTotalExpenses) * 100 
                    : 0;

                result.SavingsChange = previousSavings != 0 
                    ? ((result.TotalSavings - previousSavings) / Math.Abs(previousSavings)) * 100 
                    : 0;

                result.SavingsRateChange = previousSavingsRate != 0 
                    ? ((result.SavingsRate - previousSavingsRate) / Math.Abs(previousSavingsRate)) * 100 
                    : 0;

                // Income change (assuming income is constant)
                result.IncomeChange = 0;
            }

            // Group expenses by category
            var categoryGroups = currentExpenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key.ToString(),
                    Amount = g.Sum(e => e.Amount)
                })
                .ToList();

            // Calculate expense breakdown
            foreach (var group in categoryGroups)
            {
                result.ExpenseBreakdown.Add(new ExpenseCategoryBreakdownDto
                {
                    Category = group.Category,
                    Amount = group.Amount,
                    Percentage = result.TotalExpenses > 0 ? (group.Amount / result.TotalExpenses) * 100 : 0
                });
            }

            // Generate recommendations
            if (result.HasSufficientData)
            {
                result.Recommendations = await _recommendationsService.GenerateRecommendationsAsync(userId, year, month);
            }

            return result;
        }
    }
}
