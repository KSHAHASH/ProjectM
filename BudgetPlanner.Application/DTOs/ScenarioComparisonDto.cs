using BudgetPlanner.Application.DTOs;

namespace BudgetPlanner.Application.DTOs
{
    public class ScenarioComparisonDto
    {
        public string ScenarioName { get; set; } = string.Empty;
        public string ScenarioDescription { get; set; } = string.Empty;
        
        // Baseline (current) values
        public decimal BaselineIncome { get; set; }
        public decimal BaselineExpenses { get; set; }
        public FinancialHealthDto BaselineHealth { get; set; } = new();
        
        // Scenario (simulated) values
        public decimal ScenarioIncome { get; set; }
        public decimal ScenarioExpenses { get; set; }
        public FinancialHealthDto ScenarioHealth { get; set; } = new();
        
        // Impact analysis
        public decimal IncomeDifference { get; set; }
        public decimal ExpensesDifference { get; set; }
        public decimal SavingsDifference { get; set; }
        public decimal SavingsRateDifference { get; set; }
        public string HealthStatusChange { get; set; } = string.Empty;
        
        // Goal feasibility impact
        public List<GoalFeasibilityComparison> GoalImpacts { get; set; } = new();
        
        // Overall assessment
        public string ImpactSeverity { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }
    
    public class GoalFeasibilityComparison
    {
        public string GoalTitle { get; set; } = string.Empty;
        public string BaselineStatus { get; set; } = string.Empty;
        public string ScenarioStatus { get; set; } = string.Empty;
        public decimal BaselineSurplusAfterGoal { get; set; }
        public decimal ScenarioSurplusAfterGoal { get; set; }
        public bool StatusChanged { get; set; }
        public string Impact { get; set; } = string.Empty;
    }
}
