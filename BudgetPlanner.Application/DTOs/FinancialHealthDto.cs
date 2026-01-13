namespace BudgetPlanner.Application.DTOs
{
    public class FinancialHealthDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal SavingsAmount { get; set; }
        public decimal SavingsRate { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }
}
