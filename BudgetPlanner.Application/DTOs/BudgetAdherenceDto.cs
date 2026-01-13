namespace BudgetPlanner.Application.DTOs
{
    public class BudgetAdherenceDto
    {
        public decimal ActualAmount { get; set; }
        public decimal BudgetLimit { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public bool IsWithinBudget { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
