using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.Application.Interfaces
{
    public interface IAnalysisService
    {
        FinancialHealthDto CalculateFinancialHealth(decimal income, IEnumerable<decimal> expenses);
        BudgetAdherenceDto CalculateBudgetAdherence(decimal actual, decimal budgetLimit);
        SpendingBehaviorDto AnalyzeSpendingBehavior(IEnumerable<ExpenseDto> expenses);
        
        /// <summary>
        /// New method that calculates financial health AND saves data to database.
        /// Returns a Task because it performs async database operations.
        /// </summary>
        Task<FinancialHealthDto> CalculateAndSaveFinancialHealthAsync(
            decimal income, 
            IEnumerable<ExpenseDto> expenseDtos, 
            int userId);
    }
}
