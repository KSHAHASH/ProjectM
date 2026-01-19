using BudgetPlanner.Application.DTOs;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlanner.Application.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly ApplicationDbContext _dbContext;
        public AnalysisService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<FinancialHealthDto> CalculateAndSaveFinancialHealthAsync(
            decimal income, 
            IEnumerable<ExpenseDto> expenseDtos, 
            int userId)
        {
            var expenseList = expenseDtos.ToList();
            
            // Extract amounts for calculation
            var expenseAmounts = expenseList.Select(e => e.Amount);
            var totalExpense = expenseAmounts.Sum();
            
            // Calculate financial health using existing method
            var healthDto = CalculateFinancialHealth(income, expenseAmounts);
            
        // Get the user from database
        var user = await _dbContext.Users.FindAsync(userId);
        if (user != null)
        {
            user.MonthlyIncome = income;
            user.TotalIncome += income; // Update total income
            user.AvailableBalance = user.AvailableBalance - totalExpense;
            _dbContext.Users.Update(user);
        }
        
        // Convert ExpenseDtos to Expense entities and save to database
        // Converting from DTOs (API layer) to Entities (Database layer)
        var expenses = expenseList.Select(e => new Expense
        {
            UserId = userId,
            Category = e.Category,
            Amount = e.Amount,
            Date = e.Date ?? DateTime.UtcNow, // Use provided date or current date
            Type = e.Type // Include expense type
        }).ToList();
        
        // Add all expenses to DbContext (marks for insertion)
        _dbContext.Expenses.AddRange(expenses);
        
        // Save changes to database - this executes the INSERT SQL statements
        await _dbContext.SaveChangesAsync();
        
        return healthDto;
    }

    public FinancialHealthDto CalculateFinancialHealth(decimal income, IEnumerable<decimal> expenses)
    {
        var totalExpenses = expenses.Sum();
        
        // Formula: Savings = Income - Total Expenses
        var savingsAmount = income - totalExpenses;
        
        // Formula: Savings Rate = (Savings / Income) * 100
        // Represents the percentage of income being saved
        var savingsRate = income > 0 ? (savingsAmount / income) * 100 : 0;
        
        // Formula: Expense Ratio = (Total Expenses / Income) * 100
        // Represents the percentage of income being spent
        var expenseRatio = income > 0 ? (totalExpenses / income) * 100 : 0;
        
        // Formula: Financial Health Score = weighted calculation
        // Score ranges from 0-100
        // - Savings Rate contributes 50% (higher is better)
        // - Expense Ratio contributes 50% (lower is better, so we invert it)
        // substracting from 100 results to the good score being higher
        var healthScore = (savingsRate * 0.5m) + ((100 - expenseRatio) * 0.5m);
        
        // Determine health status based on score thresholds
        string healthStatus;
        string recommendation;
        
        if (healthScore >= 80)
        {
            healthStatus = "Excellent";
            recommendation = "You're managing your finances exceptionally well. Consider increasing your investments.";
        }
        else if (healthScore >= 60)
        {
            healthStatus = "Good";
            recommendation = "Your financial health is solid. Look for opportunities to reduce expenses and increase savings.";
        }
        else if (healthScore >= 40)
        {
            healthStatus = "Fair";
            recommendation = "Your finances need attention. Review your expenses and create a stricter budget.";
        }
        else if (healthScore >= 20)
        {
            healthStatus = "Poor";
            recommendation = "Immediate action required. Significantly reduce discretionary spending and seek financial advice.";
        }
        else
        {
            healthStatus = "Critical";
            recommendation = "Urgent financial intervention needed. Consider consulting a financial advisor immediately.";
        }
        
        return new FinancialHealthDto
        {
            TotalIncome = income,
            TotalExpenses = totalExpenses,
            SavingsAmount = savingsAmount,
            SavingsRate = Math.Round(savingsRate, 2),
            HealthStatus = healthStatus,
            Recommendation = recommendation
        };
    }

    public BudgetAdherenceDto CalculateBudgetAdherence(decimal actual, decimal budgetLimit)
    {
        // Formula: Variance = Actual - Budget Limit
        // Positive variance means over budget, negative means under budget
        var variance = actual - budgetLimit;
        
        // Formula: Variance Percentage = (Variance / Budget Limit) * 100
        var variancePercentage = budgetLimit > 0 ? (variance / budgetLimit) * 100 : 0;
        
        // Determine if spending is within budget
        var isWithinBudget = actual <= budgetLimit;
        
        // Determine status based on variance percentage
        string status;
        if (isWithinBudget)
        {
            if (variancePercentage <= -20)
            {
                status = "Well Under Budget";
            }
            else if (variancePercentage <= -10)
            {
                status = "Under Budget";
            }
            else
            {
                status = "Within Budget";
            }
        }
        else
        {
            if (variancePercentage >= 50)
            {
                status = "Severely Over Budget";
            }
            else if (variancePercentage >= 25)
            {
                status = "Significantly Over Budget";
            }
            else if (variancePercentage >= 10)
            {
                status = "Over Budget";
            }
            else
            {
                status = "Slightly Over Budget";
            }
        }
        
        return new BudgetAdherenceDto
        {
            ActualAmount = actual,
            BudgetLimit = budgetLimit,
            Variance = Math.Round(variance, 2),
            VariancePercentage = Math.Round(variancePercentage, 2),
            IsWithinBudget = isWithinBudget,
            Status = status
        };
    }

        public SpendingBehaviorDto AnalyzeSpendingBehavior(IEnumerable<ExpenseDto> expenses)
        {
            // use ToList to avoid multiple enumerations; WHEN YOU NEED TO REUSE DATA MULTIPLE TIMES
            var expenseList = expenses.ToList();
            
            if (!expenseList.Any())
            {
                return new SpendingBehaviorDto
                {
                    CategoryBreakdown = new Dictionary<ExpenseCategory, decimal>(),
                    TopCategory = ExpenseCategory.Other,
                    TopCategoryAmount = 0,
                    AverageExpenseAmount = 0,
                    TotalTransactions = 0,
                    TypeDistribution = new Dictionary<ExpenseType, int>(),
                    Insights = new List<string> { "No expense data available for analysis." }
                };
            }
            
            // Formula: Category Breakdown = Sum of expenses grouped by category
            var categoryBreakdown = expenseList
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
            
            // Formula: Category Dominance = Category with highest total spending
            var topCategory = categoryBreakdown.OrderByDescending(kvp => kvp.Value).First();
            
            // Formula: Average Expense = Total Amount / Number of Transactions
            var averageExpenseAmount = expenseList.Average(e => e.Amount);
            
            // Formula: Type Distribution = Count of expenses by type
            var typeDistribution = expenseList
                .GroupBy(e => e.Type)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Calculate total spending for percentage calculations
            var totalSpending = expenseList.Sum(e => e.Amount);
            
            // Generate insights based on spending patterns
            var insights = new List<string>();
            
            // Insight 1: Category dominance
            var topCategoryPercentage = (topCategory.Value / totalSpending) * 100;
            if (topCategoryPercentage > 40)
            {
                insights.Add($"{topCategory.Key} dominates your spending at {topCategoryPercentage:F1}% of total expenses.");
            }
            
            // Insight 2: High average transaction
            if (averageExpenseAmount > 100)
            {
                insights.Add($"Your average transaction amount is ${averageExpenseAmount:F2}, which is relatively high.");
            }
            
            // Insight 3: Fixed vs Variable expense analysis
            if (typeDistribution.ContainsKey(ExpenseType.Fixed) && typeDistribution.ContainsKey(ExpenseType.Variable))
            {
                // typeDistribution contains the list of expense based on their types (fixed, variable,)
                var fixedCount = typeDistribution[ExpenseType.Fixed];
                var variableCount = typeDistribution[ExpenseType.Variable];
                var fixedPercentage = ((decimal)fixedCount / expenseList.Count) * 100;
                
                if (fixedPercentage > 60)
                {
                    insights.Add($"Fixed expenses make up {fixedPercentage:F1}% of your transactions, limiting budget flexibility.");
                }
                else if (fixedPercentage < 30)
                {
                    insights.Add($"Variable expenses dominate at {100 - fixedPercentage:F1}%, offering opportunities for cost reduction.");
                }
            }
            
            // Insight 4: Transaction frequency
            if (expenseList.Count > 50)
            {
                insights.Add($"High transaction frequency ({expenseList.Count} transactions) suggests frequent spending habits.");
            }
            else if (expenseList.Count < 10)
            {
                insights.Add("Low transaction frequency indicates consolidated or infrequent spending.");
            }
            
            // Insight 5: Category diversity
            var categoryCount = categoryBreakdown.Count;
            if (categoryCount <= 3)
            {
                insights.Add($"Spending is concentrated in only {categoryCount} categories, showing focused expenses.");
            }
            else if (categoryCount >= 7)
            {
                insights.Add($"Expenses span {categoryCount} categories, indicating diverse spending patterns.");
            }
            
            // Default insight if no patterns detected
            if (!insights.Any())
            {
                insights.Add("Your spending patterns appear balanced and consistent.");
            }
            
            return new SpendingBehaviorDto
            {
                CategoryBreakdown = categoryBreakdown,
                TopCategory = topCategory.Key,
                TopCategoryAmount = Math.Round(topCategory.Value, 2),
                AverageExpenseAmount = Math.Round(averageExpenseAmount, 2),
                TotalTransactions = expenseList.Count,
                TypeDistribution = typeDistribution,
                Insights = insights
            };
        }

        /// <summary>
        /// Get dashboard data for a specific user including income, balance, total expenses, 
        /// and monthly expense breakdown with LOW/MEDIUM/HIGH levels
        /// </summary>
        public async Task<DashboardDto> GetDashboardDataAsync(int userId)
        {
            // Get the user to retrieve their income
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found.");
            }

            // Get all expenses for the user
            var expenses = await _dbContext.Expenses
                .Where(e => e.UserId == userId)
                .OrderBy(e => e.Date)
                .ToListAsync();

            // Calculate total expenses
            var totalExpenses = expenses.Sum(e => e.Amount);

            // Calculate available balance
            var availableBalance = user.TotalIncome - totalExpenses;

            // Group expenses by month and calculate totals
            var monthlyExpenses = expenses
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(e => e.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Select(x =>
                {
                    var monthName = new DateTime(x.Year, x.Month, 1).ToString("MMM");
                    // Calculate percentage based on monthly income
                    var percentage = user.MonthlyIncome > 0 ? (x.Amount / user.MonthlyIncome) * 100 : 0;
                    
                    // Determine expense level based on percentage of monthly income
                    // High: > 50%, Medium: 30-50%, Low: < 30%
                    ExpenseLevel level;
                    if (percentage > 50)
                    {
                        level = ExpenseLevel.High;
                    }
                    else if (percentage >= 30)
                    {
                        level = ExpenseLevel.Medium;
                    }
                    else
                    {
                        level = ExpenseLevel.Low;
                    }

                    return new MonthlyExpenseDto
                    {
                        Month = monthName,
                        Amount = Math.Round(x.Amount, 2),
                        Level = level
                    };
                })
                .ToList();

            return new DashboardDto
            {
                TotalIncome = user.TotalIncome,
                TotalExpenses = Math.Round(totalExpenses, 2),
                AvailableBalance = Math.Round(availableBalance, 2),
                MonthlyExpenses = monthlyExpenses
            };
        }
    }
}
