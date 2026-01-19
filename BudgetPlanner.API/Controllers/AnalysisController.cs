using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Application.DTOs;
using BudgetPlanner.Application.Services;
using System.Security.Claims;

namespace BudgetPlanner.API.Controllers
{
    public class DashboardRequest
    {
        public decimal Income { get; set; }
        public List<ExpenseDto> Expenses { get; set; } = new();
    }

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;
        private readonly IMonthlyAnalysisService _monthlyAnalysisService;

        public AnalysisController(IAnalysisService analysisService, IMonthlyAnalysisService monthlyAnalysisService)
        {
            _analysisService = analysisService;
            _monthlyAnalysisService = monthlyAnalysisService;
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

            // Get authenticated user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User not authenticated");
            }
            int userId = int.Parse(userIdClaim);
            
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
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            try
            {
                // Get authenticated user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized("User not authenticated");
                }
                int userId = int.Parse(userIdClaim);
                
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

        [HttpGet("monthly")]
        public async Task<ActionResult<MonthlyAnalysisDto>> GetMonthlyAnalysis([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                // Validate input
                if (year < 2000 || year > 2100)
                {
                    return BadRequest("Invalid year");
                }

                if (month < 1 || month > 12)
                {
                    return BadRequest("Invalid month");
                }

                // Get authenticated user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized("User not authenticated");
                }
                int userId = int.Parse(userIdClaim);
                
                var result = await _monthlyAnalysisService.GetMonthlyAnalysisAsync(userId, year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
