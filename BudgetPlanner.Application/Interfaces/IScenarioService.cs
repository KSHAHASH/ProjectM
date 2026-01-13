using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.Application.Interfaces
{
    public interface IScenarioService
    {
        ScenarioComparisonDto SimulateIncomeReduction(
            decimal currentIncome,
            decimal currentExpenses,
            decimal reductionPercentage,
            IEnumerable<GoalDto>? goals = null);
        
        ScenarioComparisonDto SimulateExpenseIncrease(
            decimal currentIncome,
            decimal currentExpenses,
            decimal increaseAmount,
            IEnumerable<GoalDto>? goals = null);
        
        ScenarioComparisonDto SimulateCustomScenario(
            decimal currentIncome,
            decimal currentExpenses,
            decimal newIncome,
            decimal newExpenses,
            string scenarioName,
            IEnumerable<GoalDto>? goals = null);
    }
}
