using Microsoft.AspNetCore.Mvc;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        /// <summary>
        /// Get financial health dashboard overview
        /// </summary>
        /// <param name="income">Monthly income</param>
        /// <param name="expenses">List of expense amounts</param>
        /// <returns>Financial health analysis</returns>
        [HttpGet("dashboard")]
        public ActionResult<FinancialHealthDto> GetDashboard([FromQuery] decimal income, [FromQuery] decimal[] expenses)
        {
            if (income <= 0)
            {
                return BadRequest("Income must be greater than zero.");
            }

            var result = _analysisService.CalculateFinancialHealth(income, expenses);
            return Ok(result);
        }

        [HttpPost("expenses")]
        public ActionResult<ExpenseDto> PostData([FromBody] ExpenseDto data)
        {
            if (data.Income <= 0)
            {
                return BadRequest("Income must be greater than zero.");
            }

            var result = _analysisService.CalculateFinancialHealth(data.Income, new[] { data.Amount });
            return Ok(result);
        }

        /// <summary>
        /// Get budget adherence analysis
        /// </summary>
        /// <param name="actual">Actual spending amount</param>
        /// <param name="budgetLimit">Budget limit amount</param>
        /// <returns>Budget adherence analysis</returns>
        [HttpGet("budget")]
        public ActionResult<BudgetAdherenceDto> GetBudgetAdherence([FromQuery] decimal actual, [FromQuery] decimal budgetLimit)
        {
            if (budgetLimit <= 0)
            {
                return BadRequest("Budget limit must be greater than zero.");
            }

            if (actual < 0)
            {
                return BadRequest("Actual amount cannot be negative.");
            }

            var result = _analysisService.CalculateBudgetAdherence(actual, budgetLimit);
            return Ok(result);
        }

        /// <summary>
        /// Get spending behavior analysis
        /// </summary>
        /// <param name="expenses">List of expenses</param>
        /// <returns>Spending behavior analysis</returns>
        [HttpPost("behavior")]
        public ActionResult<SpendingBehaviorDto> GetSpendingBehavior([FromBody] List<ExpenseDto> expenses)
        {
            if (expenses == null || !expenses.Any())
            {
                return BadRequest("At least one expense is required for analysis.");
            }

            var result = _analysisService.AnalyzeSpendingBehavior(expenses);
            return Ok(result);
        }
    }
}
