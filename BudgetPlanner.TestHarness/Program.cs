using BudgetPlanner.Domain.Entities;
using BudgetPlanner.Domain.Enums;
using BudgetPlanner.Application.Services;
using BudgetPlanner.Application.DTOs;

Console.WriteLine("=== Budget Planner Test Harness ===\n");

// Create a sample user with monthly income of $5000
var user = new User
{
    Id = 1,
    Name = "John Doe",
    Email = "john.doe@example.com",
    MonthlyIncome = 5000m
};

Console.WriteLine($"User: {user.Name}");
Console.WriteLine($"Email: {user.Email}");
Console.WriteLine($"Monthly Income: ${user.MonthlyIncome:F2}\n");

// Create sample expenses
var expenses = new List<Expense>
{
    // Rent - Fixed expense
    new Expense
    {
        Id = 1,
        UserId = user.Id,
        Category = ExpenseCategory.Housing,
        Amount = 1800m,
        Date = DateTime.Now.AddDays(-10),
        Type = ExpenseType.Fixed,
        User = user
    },
    
    // Food - Variable expense
    new Expense
    {
        Id = 2,
        UserId = user.Id,
        Category = ExpenseCategory.Food,
        Amount = 600m,
        Date = DateTime.Now.AddDays(-8),
        Type = ExpenseType.Variable,
        User = user
    },
    
    // Transport - Fixed expense
    new Expense
    {
        Id = 3,
        UserId = user.Id,
        Category = ExpenseCategory.Transportation,
        Amount = 300m,
        Date = DateTime.Now.AddDays(-5),
        Type = ExpenseType.Fixed,
        User = user
    },
    
    // Entertainment - Variable expense
    new Expense
    {
        Id = 4,
        UserId = user.Id,
        Category = ExpenseCategory.Entertainment,
        Amount = 400m,
        Date = DateTime.Now.AddDays(-3),
        Type = ExpenseType.Variable,
        User = user
    },
    
    // Savings - Fixed expense
    new Expense
    {
        Id = 5,
        UserId = user.Id,
        Category = ExpenseCategory.Savings,
        Amount = 500m,
        Date = DateTime.Now.AddDays(-1),
        Type = ExpenseType.Fixed,
        User = user
    }
};

Console.WriteLine("Sample Expenses:");
Console.WriteLine(new string('-', 80));
Console.WriteLine($"{"Category",-20} {"Amount",-15} {"Type",-15} {"Date",-20}");
Console.WriteLine(new string('-', 80));

var totalExpenses = 0m;
foreach (var expense in expenses)
{
    Console.WriteLine($"{expense.Category,-20} ${expense.Amount,-14:F2} {expense.Type,-15} {expense.Date:yyyy-MM-dd}");
    totalExpenses += expense.Amount;
}

Console.WriteLine(new string('-', 80));
Console.WriteLine($"{"Total Expenses:",-20} ${totalExpenses,-14:F2}");
Console.WriteLine($"{"Remaining:",-20} ${user.MonthlyIncome - totalExpenses,-14:F2}");
Console.WriteLine();

// ============================================================================
// TEST ANALYSIS SERVICE
// ============================================================================

Console.WriteLine("\n=== TESTING ANALYSIS SERVICE ===\n");

// Instantiate AnalysisService
var analysisService = new AnalysisService();

// Test 1: Calculate Financial Health
Console.WriteLine("1. Financial Health Analysis:");
Console.WriteLine(new string('-', 80));

var expenseAmounts = expenses.Select(e => e.Amount).ToList();
var financialHealth = analysisService.CalculateFinancialHealth(user.MonthlyIncome, expenseAmounts);

Console.WriteLine($"Total Income:        ${financialHealth.TotalIncome:F2}");
Console.WriteLine($"Total Expenses:      ${financialHealth.TotalExpenses:F2}");
Console.WriteLine($"Savings Amount:      ${financialHealth.SavingsAmount:F2}");
Console.WriteLine($"Savings Rate:        {financialHealth.SavingsRate:F2}%");
Console.WriteLine($"Health Status:       {financialHealth.HealthStatus}");
Console.WriteLine($"Recommendation:      {financialHealth.Recommendation}");
Console.WriteLine();

// Test 2: Calculate Budget Adherence
Console.WriteLine("2. Budget Adherence Analysis:");
Console.WriteLine(new string('-', 80));

var housingBudget = 2000m;
var actualHousing = expenses.Where(e => e.Category == ExpenseCategory.Housing).Sum(e => e.Amount);
var budgetAdherence = analysisService.CalculateBudgetAdherence(actualHousing, housingBudget);

Console.WriteLine($"Budget Limit:        ${budgetAdherence.BudgetLimit:F2}");
Console.WriteLine($"Actual Amount:       ${budgetAdherence.ActualAmount:F2}");
Console.WriteLine($"Variance:            ${budgetAdherence.Variance:F2}");
Console.WriteLine($"Variance %:          {budgetAdherence.VariancePercentage:F2}%");
Console.WriteLine($"Within Budget:       {budgetAdherence.IsWithinBudget}");
Console.WriteLine($"Status:              {budgetAdherence.Status}");
Console.WriteLine();

// Test 3: Analyze Spending Behavior
Console.WriteLine("3. Spending Behavior Analysis:");
Console.WriteLine(new string('-', 80));

var expenseDtos = expenses.Select(e => new ExpenseDto
{
    Id = e.Id,
    UserId = e.UserId,
    Category = e.Category,
    Amount = e.Amount,
    Date = e.Date,
    Type = e.Type
}).ToList();

var spendingBehavior = analysisService.AnalyzeSpendingBehavior(expenseDtos);

Console.WriteLine($"Total Transactions:  {spendingBehavior.TotalTransactions}");
Console.WriteLine($"Average Expense:     ${spendingBehavior.AverageExpenseAmount:F2}");
Console.WriteLine($"Top Category:        {spendingBehavior.TopCategory} (${spendingBehavior.TopCategoryAmount:F2})");
Console.WriteLine();

Console.WriteLine("Category Breakdown:");
foreach (var category in spendingBehavior.CategoryBreakdown.OrderByDescending(c => c.Value))
{
    var percentage = (category.Value / spendingBehavior.CategoryBreakdown.Values.Sum()) * 100;
    Console.WriteLine($"  {category.Key,-20} ${category.Value,-10:F2} ({percentage:F1}%)");
}
Console.WriteLine();

Console.WriteLine("Type Distribution:");
foreach (var type in spendingBehavior.TypeDistribution)
{
    Console.WriteLine($"  {type.Key,-20} {type.Value} transactions");
}
Console.WriteLine();

Console.WriteLine("Insights:");
foreach (var insight in spendingBehavior.Insights)
{
    Console.WriteLine($"  • {insight}");
}
Console.WriteLine();

// ============================================================================
// TEST GOAL SERVICE
// ============================================================================

Console.WriteLine("\n=== TESTING GOAL SERVICE ===\n");

// Instantiate GoalService
var goalService = new GoalService();

// Create sample goals
var goals = new List<GoalDto>
{
    new GoalDto
    {
        Id = 1,
        UserId = user.Id,
        Title = "Emergency Fund",
        TargetAmount = 10000m,
        CurrentSaved = 2000m,
        Deadline = DateTime.Now.AddMonths(12)
    },
    new GoalDto
    {
        Id = 2,
        UserId = user.Id,
        Title = "Vacation",
        TargetAmount = 3000m,
        CurrentSaved = 500m,
        Deadline = DateTime.Now.AddMonths(6)
    },
    new GoalDto
    {
        Id = 3,
        UserId = user.Id,
        Title = "New Laptop",
        TargetAmount = 2000m,
        CurrentSaved = 0m,
        Deadline = DateTime.Now.AddMonths(4)
    }
};

Console.WriteLine("Goal Feasibility Analysis:");
Console.WriteLine(new string('=', 80));

foreach (var goal in goals)
{
    var feasibility = goalService.EvaluateGoalFeasibility(goal, user.MonthlyIncome, totalExpenses);
    
    Console.WriteLine($"\nGoal: {feasibility.GoalTitle}");
    Console.WriteLine(new string('-', 80));
    Console.WriteLine($"Target Amount:              ${feasibility.TargetAmount:F2}");
    Console.WriteLine($"Current Saved:              ${feasibility.CurrentSaved:F2}");
    Console.WriteLine($"Remaining Amount:           ${feasibility.RemainingAmount:F2}");
    Console.WriteLine($"Deadline:                   {feasibility.Deadline:yyyy-MM-dd}");
    Console.WriteLine($"Months Remaining:           {feasibility.MonthsRemaining}");
    Console.WriteLine($"Required Monthly Savings:   ${feasibility.RequiredMonthlySavings:F2}");
    Console.WriteLine($"Available Surplus:          ${feasibility.AvailableSurplus:F2}");
    Console.WriteLine($"Surplus After Goal:         ${feasibility.SurplusAfterGoal:F2}");
    Console.WriteLine($"Feasibility Status:         {feasibility.FeasibilityStatus}");
    Console.WriteLine($"Feasibility Score:          {feasibility.FeasibilityScore:F2}");
    Console.WriteLine($"Recommendation:             {feasibility.Recommendation}");
}
Console.WriteLine();

// ============================================================================
// TEST SCENARIO SERVICE
// ============================================================================

Console.WriteLine("\n=== TESTING SCENARIO SERVICE ===\n");

// Instantiate ScenarioService (depends on AnalysisService and GoalService)
var scenarioService = new ScenarioService(analysisService, goalService);

// Test 1: Income Reduction Scenario
Console.WriteLine("1. Income Reduction Scenario (20% reduction):");
Console.WriteLine(new string('=', 80));

var incomeReductionScenario = scenarioService.SimulateIncomeReduction(
    user.MonthlyIncome, 
    totalExpenses, 
    20m, 
    goals);

Console.WriteLine($"Scenario: {incomeReductionScenario.ScenarioName}");
Console.WriteLine($"Description: {incomeReductionScenario.ScenarioDescription}");
Console.WriteLine();

Console.WriteLine("Financial Impact:");
Console.WriteLine(new string('-', 80));
Console.WriteLine($"Baseline Income:            ${incomeReductionScenario.BaselineIncome:F2}");
Console.WriteLine($"Scenario Income:            ${incomeReductionScenario.ScenarioIncome:F2}");
Console.WriteLine($"Income Difference:          ${incomeReductionScenario.IncomeDifference:F2}");
Console.WriteLine($"Baseline Savings:           ${incomeReductionScenario.BaselineHealth.SavingsAmount:F2}");
Console.WriteLine($"Scenario Savings:           ${incomeReductionScenario.ScenarioHealth.SavingsAmount:F2}");
Console.WriteLine($"Savings Difference:         ${incomeReductionScenario.SavingsDifference:F2}");
Console.WriteLine($"Savings Rate Change:        {incomeReductionScenario.SavingsRateDifference:F2}%");
Console.WriteLine($"Health Status Change:       {incomeReductionScenario.HealthStatusChange}");
Console.WriteLine($"Impact Severity:            {incomeReductionScenario.ImpactSeverity}");
Console.WriteLine();

if (incomeReductionScenario.GoalImpacts.Any())
{
    Console.WriteLine("Goal Impacts:");
    Console.WriteLine(new string('-', 80));
    foreach (var goalImpact in incomeReductionScenario.GoalImpacts)
    {
        Console.WriteLine($"  {goalImpact.GoalTitle}:");
        Console.WriteLine($"    Baseline Status: {goalImpact.BaselineStatus}");
        Console.WriteLine($"    Scenario Status: {goalImpact.ScenarioStatus}");
        Console.WriteLine($"    Impact: {goalImpact.Impact}");
    }
    Console.WriteLine();
}

Console.WriteLine("Recommendations:");
foreach (var recommendation in incomeReductionScenario.Recommendations)
{
    Console.WriteLine($"  • {recommendation}");
}
Console.WriteLine();

// Test 2: Expense Increase Scenario
Console.WriteLine("\n2. Expense Increase Scenario (+$500/month):");
Console.WriteLine(new string('=', 80));

var expenseIncreaseScenario = scenarioService.SimulateExpenseIncrease(
    user.MonthlyIncome, 
    totalExpenses, 
    500m, 
    goals);

Console.WriteLine($"Scenario: {expenseIncreaseScenario.ScenarioName}");
Console.WriteLine($"Description: {expenseIncreaseScenario.ScenarioDescription}");
Console.WriteLine();

Console.WriteLine("Financial Impact:");
Console.WriteLine(new string('-', 80));
Console.WriteLine($"Baseline Expenses:          ${expenseIncreaseScenario.BaselineExpenses:F2}");
Console.WriteLine($"Scenario Expenses:          ${expenseIncreaseScenario.ScenarioExpenses:F2}");
Console.WriteLine($"Expense Difference:         ${expenseIncreaseScenario.ExpensesDifference:F2}");
Console.WriteLine($"Baseline Savings:           ${expenseIncreaseScenario.BaselineHealth.SavingsAmount:F2}");
Console.WriteLine($"Scenario Savings:           ${expenseIncreaseScenario.ScenarioHealth.SavingsAmount:F2}");
Console.WriteLine($"Savings Difference:         ${expenseIncreaseScenario.SavingsDifference:F2}");
Console.WriteLine($"Impact Severity:            {expenseIncreaseScenario.ImpactSeverity}");
Console.WriteLine();

Console.WriteLine("Recommendations:");
foreach (var recommendation in expenseIncreaseScenario.Recommendations)
{
    Console.WriteLine($"  • {recommendation}");
}
Console.WriteLine();

Console.WriteLine("\n=== Test Harness Complete ===");
