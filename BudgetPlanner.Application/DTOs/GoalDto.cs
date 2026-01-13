namespace BudgetPlanner.Application.DTOs
{
    public class GoalDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal CurrentSaved { get; set; }
        public DateTime Deadline { get; set; }
    }
}
