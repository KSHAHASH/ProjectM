using BudgetPlanner.Application.DTOs;
using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Application.Services
{
    public interface IRecommendationsService
    {
        Task<List<RecommendationDto>> GenerateRecommendationsAsync(int userId, int year, int month);
    }

    public class RecommendationsService : IRecommendationsService
    {
        private readonly ApplicationDbContext _context;
        private const decimal SignificantChangeThreshold = 10m; // 10% change is significant

        public RecommendationsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RecommendationDto>> GenerateRecommendationsAsync(int userId, int year, int month)
        {
            var recommendations = new List<RecommendationDto>();

            // Get current month data
            var currentMonthStart = new DateTime(year, month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

            // Get previous month data
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);

            // Get user
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return recommendations;

            // Get current month expenses
            var currentExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= currentMonthStart && e.Date <= currentMonthEnd)
                .ToListAsync();

            // Get previous month expenses
            var previousExpenses = await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= previousMonthStart && e.Date <= previousMonthEnd)
                .ToListAsync();

            // Check if we have sufficient data (at least 2 months)
            if (!currentExpenses.Any() || !previousExpenses.Any())
            {
                return recommendations; // Return empty list if insufficient data
            }

            // Calculate totals
            var currentTotalExpenses = currentExpenses.Sum(e => e.Amount);
            var previousTotalExpenses = previousExpenses.Sum(e => e.Amount);

            var currentSavings = user.MonthlyIncome - currentTotalExpenses;
            var previousSavings = user.MonthlyIncome - previousTotalExpenses;

            // 1. Overall Savings Comparison
            if (currentSavings > previousSavings)
            {
                var savingsDiff = currentSavings - previousSavings;
                recommendations.Add(new RecommendationDto
                {
                    Type = "success",
                    Icon = "down",
                    Title = "Great job!",
                    Message = "Keep it up to reach your savings goals.",
                    HighlightedValue = $"${savingsDiff:F0} more"
                });
            }
            else if (currentSavings < previousSavings)
            {
                var savingsDiff = previousSavings - currentSavings;
                recommendations.Add(new RecommendationDto
                {
                    Type = "warning",
                    Icon = "up",
                    Title = "Savings decreased",
                    Message = $"You saved ${savingsDiff:F0} less than last month. Consider reviewing your expenses.",
                    HighlightedValue = $"-${savingsDiff:F0}"
                });
            }

            // 2. Category-wise Analysis
            var currentByCategory = currentExpenses.GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key.ToString(), Amount = g.Sum(e => e.Amount) })
                .ToList();

            var previousByCategory = previousExpenses.GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key.ToString(), Amount = g.Sum(e => e.Amount) })
                .ToList();

            foreach (var currentCat in currentByCategory)
            {
                var previousCat = previousByCategory.FirstOrDefault(p => p.Category == currentCat.Category);
                
                if (previousCat != null && previousCat.Amount > 0)
                {
                    var percentageChange = ((currentCat.Amount - previousCat.Amount) / previousCat.Amount) * 100;
                    
                    if (percentageChange > SignificantChangeThreshold)
                    {
                        recommendations.Add(new RecommendationDto
                        {
                            Type = "warning",
                            Icon = "up",
                            Title = $"Spending Alert: {currentCat.Category}",
                            Message = $"You spent {percentageChange:F0}% more on {currentCat.Category} than last month. Consider reducing {currentCat.Category.ToLower()} expenses to stay within your budget.",
                            HighlightedValue = $"{percentageChange:F0}% more"
                        });
                    }
                    else if (percentageChange < -SignificantChangeThreshold)
                    {
                        recommendations.Add(new RecommendationDto
                        {
                            Type = "success",
                            Icon = "down",
                            Title = $"Good Progress: {currentCat.Category}",
                            Message = $"You reduced {currentCat.Category} spending by {Math.Abs(percentageChange):F0}%. Keep it up!",
                            HighlightedValue = $"{Math.Abs(percentageChange):F0}% less"
                        });
                    }
                }
            }

            // 3. Savings Rate Check
            var currentSavingsRate = user.MonthlyIncome > 0 ? (currentSavings / user.MonthlyIncome) * 100 : 0;
            
            if (currentSavingsRate >= 20)
            {
                recommendations.Add(new RecommendationDto
                {
                    Type = "success",
                    Icon = "check",
                    Title = "Excellent Savings!",
                    Message = "Keep it up! You're on track with your savings goals.",
                    HighlightedValue = $"{currentSavingsRate:F0}%"
                });
            }
            else if (currentSavingsRate < 10 && currentSavingsRate > 0)
            {
                // Find highest expense category to suggest
                var highestCategory = currentByCategory.OrderByDescending(c => c.Amount).FirstOrDefault();
                if (highestCategory != null)
                {
                    var suggestedReduction = highestCategory.Amount * 0.2m; // Suggest 20% reduction
                    recommendations.Add(new RecommendationDto
                    {
                        Type = "tip",
                        Icon = "lightbulb",
                        Title = "Savings Tip",
                        Message = $"Limit {highestCategory.Category} to ${(highestCategory.Amount - suggestedReduction):F0} next month to boost your savings. This can help you stay on track with your budget.",
                        HighlightedValue = $"${suggestedReduction:F0}"
                    });
                }
            }

            // 4. Overall expense trend
            if (currentTotalExpenses > previousTotalExpenses)
            {
                var expenseIncrease = ((currentTotalExpenses - previousTotalExpenses) / previousTotalExpenses) * 100;
                if (expenseIncrease > 15)
                {
                    recommendations.Add(new RecommendationDto
                    {
                        Type = "warning",
                        Icon = "up",
                        Title = "Total Expenses Increased",
                        Message = $"Your overall spending increased by {expenseIncrease:F0}%. Review your budget to identify areas for improvement.",
                        HighlightedValue = $"+{expenseIncrease:F0}%"
                    });
                }
            }

            return recommendations;
        }
    }
}
