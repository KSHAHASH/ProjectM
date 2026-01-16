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
            [FromBody] DashboardRequest request)
        {
            // Log the incoming request for debugging
            Console.WriteLine($"Received request - Income: {request?.Income}, Expenses count: {request?.Expenses?.Count}");
            
            if (request == null)
            {
                return BadRequest("Request body is null or invalid.");
            }

            if (request.Income <= 0)
            {
                return BadRequest("Income must be greater than zero.");
            }

            if (request.Expenses == null || !request.Expenses.Any())
            {
                return BadRequest("At least one expense is required.");
            }

            // For now, using a hardcoded demo user ID
            // In production, this would come from authentication (JWT token, session, etc.)
            // Example: var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            int userId = 1; // Demo user - replace with actual authentication
            
            // Call the async method that saves data to database
            var result = await _analysisService.CalculateAndSaveFinancialHealthAsync(
                request.Income, 
                request.Expenses, 
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

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] int userId = 1)
        {
            try
            {
                var result = await _analysisService.GetDashboardDataAsync(userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
