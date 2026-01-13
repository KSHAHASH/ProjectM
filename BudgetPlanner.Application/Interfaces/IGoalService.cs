using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.Application.Interfaces
{
    public interface IGoalService
    {
        GoalFeasibilityDto EvaluateGoalFeasibility(GoalDto goal, decimal monthlyIncome, decimal monthlyExpenses);
        IEnumerable<GoalFeasibilityDto> EvaluateMultipleGoals(IEnumerable<GoalDto> goals, decimal monthlyIncome, decimal monthlyExpenses);
    }
}
