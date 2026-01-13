using BudgetPlanner.Domain.Enums;

namespace BudgetPlanner.Domain.Entities
{
    public class BudgetRule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal MonthlyLimit { get; set; }
        
        public User? User { get; set; }
    }
}
