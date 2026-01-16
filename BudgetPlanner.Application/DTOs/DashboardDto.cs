using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Application.DTOs
{
    public class DashboardDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<MonthlyExpenseDto> MonthlyExpenses { get; set; } = new();
    }

    public class MonthlyExpenseDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ExpenseLevel Level { get; set; }
    }

    public enum ExpenseLevel
    {
        Low,
        Medium,
        High
    }
}
