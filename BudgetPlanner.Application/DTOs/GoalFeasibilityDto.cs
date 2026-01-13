namespace BudgetPlanner.Application.DTOs
{
    public class GoalFeasibilityDto
    {
        public int GoalId { get; set; }
        public string GoalTitle { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentSaved { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime Deadline { get; set; }
        public int MonthsRemaining { get; set; }
        public decimal RequiredMonthlySavings { get; set; }
        public decimal AvailableSurplus { get; set; }
        public decimal SurplusAfterGoal { get; set; }
        public string FeasibilityStatus { get; set; } = string.Empty;
        public decimal FeasibilityScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }
}
