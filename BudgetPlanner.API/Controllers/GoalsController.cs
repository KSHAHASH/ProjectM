using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Application.DTOs;
using System.Security.Claims;

namespace BudgetPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IGoalService _goalService;

    public GoalsController(IGoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpPost("analysis")]
    public IActionResult GetGoalAnalysis(
        [FromQuery] decimal monthlyIncome, 
        [FromQuery] decimal monthlyExpenses, 
        [FromBody] List<GoalDto> goals)
    {
        if (goals == null || !goals.Any())
        {
            return BadRequest("At least one goal is required.");
        }

        var result = _goalService.EvaluateMultipleGoals(goals, monthlyIncome, monthlyExpenses);
        return Ok(result);
    }
}
