using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.Application.Interfaces
{
    public interface IAnalysisService
    {
        FinancialHealthDto CalculateFinancialHealth(decimal income, IEnumerable<decimal> expenses);
        BudgetAdherenceDto CalculateBudgetAdherence(decimal actual, decimal budgetLimit);
        SpendingBehaviorDto AnalyzeSpendingBehavior(IEnumerable<ExpenseDto> expenses);
    }
}
