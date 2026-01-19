namespace BudgetPlanner.Application.DTOs
{
    public class MonthlyAnalysisDto
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal SavingsRate { get; set; }
        public decimal IncomeChange { get; set; } // Percentage change from previous month
        public decimal ExpenseChange { get; set; } // Percentage change from previous month
        public decimal SavingsChange { get; set; } // Percentage change from previous month
        public decimal SavingsRateChange { get; set; } // Percentage change from previous month
        public List<ExpenseCategoryBreakdownDto> ExpenseBreakdown { get; set; } = new();
        public List<RecommendationDto> Recommendations { get; set; } = new();
        public bool HasSufficientData { get; set; }
    }

    public class ExpenseCategoryBreakdownDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RecommendationDto
    {
        public string Type { get; set; } = string.Empty; // "warning", "success", "tip", "info"
        public string Icon { get; set; } = string.Empty; // "up", "down", "lightbulb", "check"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string HighlightedValue { get; set; } = string.Empty; // e.g., "20%", "$100"
    }
}
