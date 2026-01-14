using Microsoft.AspNetCore.Mvc;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.API.Controllers
{
    public class DashboardRequest
    {
        public decimal Income { get; set; }
        public List<ExpenseDto> Expenses { get; set; } = new();
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        [HttpPost("input")]
        public async Task<ActionResult<FinancialHealthDto>> SubmitDashboard(
            [FromQuery] decimal income, 
            [FromBody] List<ExpenseDto> expenses)
        {
            if (income <= 0)
            {
                return BadRequest("Income must be greater than zero.");
            }

            if (expenses == null || !expenses.Any())
            {
                return BadRequest("At least one expense is required.");
            }

            // For now, using a hardcoded demo user ID
            // In production, this would come from authentication (JWT token, session, etc.)
            // Example: var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int userId = 1; // Demo user - replace with actual authentication
            
            // Call the new async method that saves data to database
            var result = await _analysisService.CalculateAndSaveFinancialHealthAsync(
                income, 
                expenses, 
                userId);
                
            return Ok(result);
        }

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
