using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Application.DTOs
{
    public class SpendingBehaviorDto
    {
        public Dictionary<ExpenseCategory, decimal> CategoryBreakdown { get; set; } = new();
        public ExpenseCategory TopCategory { get; set; }
        public decimal TopCategoryAmount { get; set; }
        public decimal AverageExpenseAmount { get; set; }
        public int TotalTransactions { get; set; }
        public Dictionary<ExpenseType, int> TypeDistribution { get; set; } = new();
        public List<string> Insights { get; set; } = new();
    }
}
