using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Application.DTOs
{
    public class ExpenseDto
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
        public ExpenseType Type { get; set; }
        
        // For dashboard submission
        public decimal? Income { get; set; }
        public List<ExpenseDto>? Expenses { get; set; }
    }
}



