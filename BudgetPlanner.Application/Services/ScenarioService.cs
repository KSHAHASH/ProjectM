using BudgetPlanner.Application.DTOs;
using BudgetPlanner.Application.Interfaces;

namespace BudgetPlanner.Application.Services
{
    public class ScenarioService : IScenarioService
    {
        private readonly IAnalysisService _analysisService;
        private readonly IGoalService _goalService;

        public ScenarioService(IAnalysisService analysisService, IGoalService goalService)
        {
            _analysisService = analysisService;
            _goalService = goalService;
        }

        public ScenarioComparisonDto SimulateIncomeReduction(
            decimal currentIncome, 
            decimal currentExpenses, 
            decimal reductionPercentage,
            IEnumerable<GoalDto>? goals = null)
        {
            // Formula: New Income = Current Income × (1 - Reduction Percentage / 100)
            // Example: $5000 income with 20% reduction = $5000 × (1 - 0.20) = $4000
            var scenarioIncome = currentIncome * (1 - (reductionPercentage / 100m));
            
            var scenarioName = $"Income Reduction: {reductionPercentage}%";
            var scenarioDescription = $"Simulates the impact of a {reductionPercentage}% reduction in income from ${currentIncome:F2} to ${scenarioIncome:F2}.";
            
            return SimulateCustomScenario(
                currentIncome, 
                currentExpenses, 
                scenarioIncome, 
                currentExpenses, 
                scenarioName,
                goals);
        }

        public ScenarioComparisonDto SimulateExpenseIncrease(
            decimal currentIncome, 
            decimal currentExpenses, 
            decimal increaseAmount,
            IEnumerable<GoalDto>? goals = null)
        {
            // Formula: New Expenses = Current Expenses + Increase Amount
            // Example: $3000 expenses + $500 increase = $3500
            var scenarioExpenses = currentExpenses + increaseAmount;
            
            var scenarioName = $"Expense Increase: +${increaseAmount:F2}";
            var scenarioDescription = $"Simulates the impact of increasing monthly expenses by ${increaseAmount:F2} from ${currentExpenses:F2} to ${scenarioExpenses:F2}.";
            
            return SimulateCustomScenario(
                currentIncome, 
                currentExpenses, 
                currentIncome, 
                scenarioExpenses, 
                scenarioName,
                goals);
        }

        public ScenarioComparisonDto SimulateCustomScenario(
            decimal currentIncome,
            decimal currentExpenses,
            decimal newIncome,
            decimal newExpenses,
            string scenarioName,
            IEnumerable<GoalDto>? goals = null)
        {
            // Calculate baseline financial health
            var baselineExpensesList = new List<decimal> { currentExpenses };
            var baselineHealth = _analysisService.CalculateFinancialHealth(currentIncome, baselineExpensesList);
            
            // Calculate scenario financial health
            var scenarioExpensesList = new List<decimal> { newExpenses };
            var scenarioHealth = _analysisService.CalculateFinancialHealth(newIncome, scenarioExpensesList);
            
            // Formula: Income Difference = Scenario Income - Baseline Income
            var incomeDifference = newIncome - currentIncome;
            
            // Formula: Expenses Difference = Scenario Expenses - Baseline Expenses
            var expensesDifference = newExpenses - currentExpenses;
            
            // Formula: Savings Difference = Scenario Savings - Baseline Savings
            var savingsDifference = scenarioHealth.SavingsAmount - baselineHealth.SavingsAmount;
            
            // Formula: Savings Rate Difference = Scenario Rate - Baseline Rate
            var savingsRateDifference = scenarioHealth.SavingsRate - baselineHealth.SavingsRate;
            
            // Determine health status change
            var healthStatusChange = DetermineHealthStatusChange(baselineHealth.HealthStatus, scenarioHealth.HealthStatus);
            
            // Evaluate goal impacts if goals are provided
            var goalImpacts = new List<GoalFeasibilityComparison>();
            if (goals != null && goals.Any())
            {
                goalImpacts = EvaluateGoalImpacts(goals, currentIncome, currentExpenses, newIncome, newExpenses);
            }
            
            // Determine overall impact severity
            var impactSeverity = DetermineImpactSeverity(savingsDifference, savingsRateDifference, healthStatusChange);
            
            // Generate recommendations based on scenario results
            var recommendations = GenerateRecommendations(
                scenarioHealth, 
                baselineHealth, 
                incomeDifference, 
                expensesDifference,
                goalImpacts);
            
            return new ScenarioComparisonDto
            {
                ScenarioName = scenarioName,
                ScenarioDescription = string.IsNullOrEmpty(scenarioName) 
                    ? $"Custom scenario with income ${newIncome:F2} and expenses ${newExpenses:F2}." 
                    : scenarioName,
                BaselineIncome = currentIncome,
                BaselineExpenses = currentExpenses,
                BaselineHealth = baselineHealth,
                ScenarioIncome = newIncome,
                ScenarioExpenses = newExpenses,
                ScenarioHealth = scenarioHealth,
                IncomeDifference = Math.Round(incomeDifference, 2),
                ExpensesDifference = Math.Round(expensesDifference, 2),
                SavingsDifference = Math.Round(savingsDifference, 2),
                SavingsRateDifference = Math.Round(savingsRateDifference, 2),
                HealthStatusChange = healthStatusChange,
                GoalImpacts = goalImpacts,
                ImpactSeverity = impactSeverity,
                Recommendations = recommendations
            };
        }

        /// <summary>
        /// Evaluates the impact of a scenario on goal feasibility
        /// </summary>
        private List<GoalFeasibilityComparison> EvaluateGoalImpacts(
            IEnumerable<GoalDto> goals,
            decimal baselineIncome,
            decimal baselineExpenses,
            decimal scenarioIncome,
            decimal scenarioExpenses)
        {
            var comparisons = new List<GoalFeasibilityComparison>();
            
            foreach (var goal in goals)
            {
                // Evaluate goal under baseline conditions
                var baselineFeasibility = _goalService.EvaluateGoalFeasibility(goal, baselineIncome, baselineExpenses);
                
                // Evaluate goal under scenario conditions
                var scenarioFeasibility = _goalService.EvaluateGoalFeasibility(goal, scenarioIncome, scenarioExpenses);
                
                // Determine if status changed
                var statusChanged = baselineFeasibility.FeasibilityStatus != scenarioFeasibility.FeasibilityStatus;
                
                // Determine impact description
                var impact = DetermineGoalImpact(baselineFeasibility, scenarioFeasibility);
                
                comparisons.Add(new GoalFeasibilityComparison
                {
                    GoalTitle = goal.Title,
                    BaselineStatus = baselineFeasibility.FeasibilityStatus,
                    ScenarioStatus = scenarioFeasibility.FeasibilityStatus,
                    BaselineSurplusAfterGoal = baselineFeasibility.SurplusAfterGoal,
                    ScenarioSurplusAfterGoal = scenarioFeasibility.SurplusAfterGoal,
                    StatusChanged = statusChanged,
                    Impact = impact
                });
            }
            
            return comparisons;
        }

        /// <summary>
        /// Determines the health status change description
        /// </summary>
        private string DetermineHealthStatusChange(string baselineStatus, string scenarioStatus)
        {
            if (baselineStatus == scenarioStatus)
            {
                return $"No change (remains {baselineStatus})";
            }
            
            // Define status hierarchy for comparison
            var statusHierarchy = new Dictionary<string, int>
            {
                { "Critical", 1 },
                { "Poor", 2 },
                { "Fair", 3 },
                { "Good", 4 },
                { "Excellent", 5 }
            };
            
            var baselineRank = statusHierarchy.GetValueOrDefault(baselineStatus, 0);
            var scenarioRank = statusHierarchy.GetValueOrDefault(scenarioStatus, 0);
            
            if (scenarioRank > baselineRank)
            {
                return $"Improved: {baselineStatus} → {scenarioStatus}";
            }
            else
            {
                return $"Declined: {baselineStatus} → {scenarioStatus}";
            }
        }

        /// <summary>
        /// Determines the impact on a specific goal
        /// </summary>
        private string DetermineGoalImpact(GoalFeasibilityDto baseline, GoalFeasibilityDto scenario)
        {
            var surplusChange = scenario.SurplusAfterGoal - baseline.SurplusAfterGoal;
            
            if (baseline.FeasibilityStatus == scenario.FeasibilityStatus)
            {
                if (Math.Abs(surplusChange) < 10)
                {
                    return "Minimal impact";
                }
                else if (surplusChange > 0)
                {
                    return $"Easier to achieve (${Math.Abs(surplusChange):F2} more surplus)";
                }
                else
                {
                    return $"Harder to achieve (${Math.Abs(surplusChange):F2} less surplus)";
                }
            }
            else
            {
                // Status changed
                var statusMap = new Dictionary<string, int>
                {
                    { "Not Feasible", 1 },
                    { "At Risk", 2 },
                    { "Feasible", 3 },
                    { "Achieved", 4 }
                };
                
                var baselineRank = statusMap.GetValueOrDefault(baseline.FeasibilityStatus, 0);
                var scenarioRank = statusMap.GetValueOrDefault(scenario.FeasibilityStatus, 0);
                
                if (scenarioRank > baselineRank)
                {
                    return $"Status improved: {baseline.FeasibilityStatus} → {scenario.FeasibilityStatus}";
                }
                else
                {
                    return $"Status worsened: {baseline.FeasibilityStatus} → {scenario.FeasibilityStatus}";
                }
            }
        }

        /// <summary>
        /// Determines the overall severity of the scenario impact
        /// </summary>
        private string DetermineImpactSeverity(
            decimal savingsDifference, 
            decimal savingsRateDifference, 
            string healthStatusChange)
        {
            // If health status declined, it's at least moderate impact
            if (healthStatusChange.Contains("Declined"))
            {
                if (savingsRateDifference <= -20)
                {
                    return "Severe";
                }
                else if (savingsRateDifference <= -10)
                {
                    return "Moderate";
                }
                else
                {
                    return "Minor";
                }
            }
            else if (healthStatusChange.Contains("Improved"))
            {
                return "Positive";
            }
            else
            {
                // No status change - check magnitude of savings impact
                if (Math.Abs(savingsRateDifference) < 5)
                {
                    return "Minimal";
                }
                else if (savingsDifference < 0)
                {
                    return "Minor";
                }
                else
                {
                    return "Positive";
                }
            }
        }

        /// <summary>
        /// Generates actionable recommendations based on scenario results
        /// </summary>
        private List<string> GenerateRecommendations(
            FinancialHealthDto scenarioHealth,
            FinancialHealthDto baselineHealth,
            decimal incomeDifference,
            decimal expensesDifference,
            List<GoalFeasibilityComparison> goalImpacts)
        {
            var recommendations = new List<string>();
            
            // Recommendation based on savings change
            if (scenarioHealth.SavingsAmount < baselineHealth.SavingsAmount)
            {
                var savingsLoss = baselineHealth.SavingsAmount - scenarioHealth.SavingsAmount;
                recommendations.Add($"You would lose ${savingsLoss:F2} in monthly savings under this scenario.");
            }
            
            // Recommendation based on income reduction
            if (incomeDifference < 0)
            {
                recommendations.Add($"To maintain your current financial health, consider reducing expenses by ${Math.Abs(incomeDifference):F2}/month.");
            }
            
            // Recommendation based on expense increase
            if (expensesDifference > 0)
            {
                if (scenarioHealth.SavingsAmount < 0)
                {
                    recommendations.Add("This expense increase would push you into deficit. Find ways to offset it or avoid if possible.");
                }
                else
                {
                    recommendations.Add($"This expense increase would reduce your monthly savings to ${scenarioHealth.SavingsAmount:F2}.");
                }
            }
            
            // Recommendations based on goal impacts
            var worstenedGoals = goalImpacts.Where(g => 
                g.Impact.Contains("worsened") || g.Impact.Contains("less surplus")).ToList();
            
            if (worstenedGoals.Any())
            {
                recommendations.Add($"{worstenedGoals.Count} goal(s) would become harder or impossible to achieve. Consider adjusting goal timelines or amounts.");
            }
            
            // Positive scenario recommendations
            if (scenarioHealth.SavingsAmount > baselineHealth.SavingsAmount)
            {
                recommendations.Add($"This scenario would improve your savings by ${scenarioHealth.SavingsAmount - baselineHealth.SavingsAmount:F2}/month. Consider using this for additional goals or investments.");
            }
            
            // Health status recommendations
            if (scenarioHealth.HealthStatus == "Poor" || scenarioHealth.HealthStatus == "Critical")
            {
                recommendations.Add("Build an emergency fund of 3-6 months of expenses before this scenario occurs.");
            }
            
            return recommendations;
        }
    }
}
