using BudgetPlanner.Application.DTOs;
using BudgetPlanner.Application.Interfaces;

namespace BudgetPlanner.Application.Services
{
    public class GoalService : IGoalService
    {
        public GoalFeasibilityDto EvaluateGoalFeasibility(GoalDto goal, decimal monthlyIncome, decimal monthlyExpenses)
        {
            // Formula: Remaining Amount = Target Amount - Current Saved
            // This is how much more money needs to be saved to reach the goal
            var remainingAmount = goal.TargetAmount - goal.CurrentSaved;
            
            // Formula: Months Remaining = difference between deadline and current date in months
            // Calculate how many months are left until the deadline
            var monthsRemaining = CalculateMonthsRemaining(goal.Deadline);
            
            // Formula: Required Monthly Savings = Remaining Amount / Months Remaining
            // This is how much needs to be saved each month to reach the goal
            // If deadline has passed or no months remaining, set to remaining amount
            var requiredMonthlySavings = monthsRemaining > 0 
                ? remainingAmount / monthsRemaining 
                : remainingAmount;
            
            // Formula: Available Surplus = Monthly Income - Monthly Expenses
            // This represents the amount of money available for savings each month
            var availableSurplus = monthlyIncome - monthlyExpenses;
            
            // Formula: Surplus After Goal = Available Surplus - Required Monthly Savings
            // This shows how much money will be left after setting aside goal savings
            var surplusAfterGoal = availableSurplus - requiredMonthlySavings;
            
            // Formula: Feasibility Score = (Available Surplus / Required Monthly Savings) * 100
            // Score > 100 means easily feasible, score = 100 means exact match, score < 100 means challenging
            var feasibilityScore = requiredMonthlySavings > 0 
                ? (availableSurplus / requiredMonthlySavings) * 100 
                : 100;
            
            // Determine feasibility status based on surplus coverage
            string feasibilityStatus;
            string recommendation;
            
            if (monthsRemaining <= 0)
            {
                // Goal deadline has passed
                feasibilityStatus = "Deadline Passed";
                recommendation = "The goal deadline has passed. Consider extending the deadline or adjusting the target amount.";
            }
            else if (remainingAmount <= 0)
            {
                // Goal already achieved
                feasibilityStatus = "Achieved";
                recommendation = "Congratulations! You've already reached this goal.";
            }
            else if (surplusAfterGoal >= availableSurplus * 0.3m)
            {
                // Surplus after goal is >= 30% of available surplus (highly feasible)
                feasibilityStatus = "Feasible";
                recommendation = $"This goal is easily achievable. You'll have ${surplusAfterGoal:F2} remaining each month after saving for this goal.";
            }
            else if (surplusAfterGoal >= 0)
            {
                // Surplus after goal is positive but less than 30% of available surplus (tight but feasible)
                feasibilityStatus = "Feasible";
                recommendation = $"This goal is achievable but will leave limited surplus (${surplusAfterGoal:F2}/month). Monitor your spending carefully.";
            }
            else if (surplusAfterGoal >= availableSurplus * -0.2m)
            {
                // Deficit is less than 20% of available surplus (risky but possible with adjustments)
                feasibilityStatus = "At Risk";
                recommendation = $"This goal is challenging. You need to reduce expenses by ${Math.Abs(surplusAfterGoal):F2}/month or increase income to meet this goal comfortably.";
            }
            else if (availableSurplus <= 0)
            {
                // No surplus available (spending exceeds income)
                feasibilityStatus = "Not Feasible";
                recommendation = "You're currently spending more than you earn. Focus on reducing expenses and increasing income before pursuing this goal.";
            }
            else
            {
                // Large deficit (not feasible without significant changes)
                feasibilityStatus = "Not Feasible";
                recommendation = $"This goal requires ${requiredMonthlySavings:F2}/month but you only have ${availableSurplus:F2} available. Consider extending the deadline, reducing the target amount, or significantly cutting expenses.";
            }
            
            return new GoalFeasibilityDto
            {
                GoalId = goal.Id,
                GoalTitle = goal.Title,
                TargetAmount = goal.TargetAmount,
                CurrentSaved = goal.CurrentSaved,
                RemainingAmount = Math.Round(remainingAmount, 2),
                Deadline = goal.Deadline,
                MonthsRemaining = monthsRemaining,
                RequiredMonthlySavings = Math.Round(requiredMonthlySavings, 2),
                AvailableSurplus = Math.Round(availableSurplus, 2),
                SurplusAfterGoal = Math.Round(surplusAfterGoal, 2),
                FeasibilityStatus = feasibilityStatus,
                FeasibilityScore = Math.Round(feasibilityScore, 2),
                Recommendation = recommendation
            };
        }

        public IEnumerable<GoalFeasibilityDto> EvaluateMultipleGoals(
            IEnumerable<GoalDto> goals, 
            decimal monthlyIncome, 
            decimal monthlyExpenses)
        {
            var goalList = goals.ToList();
            var results = new List<GoalFeasibilityDto>();
            
            // Calculate base available surplus
            var availableSurplus = monthlyIncome - monthlyExpenses;
            var remainingSurplus = availableSurplus;
            
            // Sort goals by deadline (closest deadline first) for priority evaluation
            var sortedGoals = goalList.OrderBy(g => g.Deadline).ToList();
            
            foreach (var goal in sortedGoals)
            {
                // Evaluate each goal with the remaining surplus
                var feasibility = EvaluateGoalFeasibility(goal, monthlyIncome, monthlyExpenses);
                
                // Adjust surplus after accounting for this goal's requirements
                if (feasibility.FeasibilityStatus == "Feasible" || feasibility.FeasibilityStatus == "At Risk")
                {
                    remainingSurplus -= feasibility.RequiredMonthlySavings;
                }
                
                results.Add(feasibility);
            }
            
            // Add aggregate warning if pursuing all goals simultaneously
            if (results.Count > 1)
            {
                var totalRequired = results
                    .Where(r => r.FeasibilityStatus != "Achieved" && r.FeasibilityStatus != "Deadline Passed")
                    .Sum(r => r.RequiredMonthlySavings);
                
                if (totalRequired > availableSurplus)
                {
                    // Update recommendations to warn about collective unfeasibility
                    foreach (var result in results)
                    {
                        if (result.FeasibilityStatus == "Feasible")
                        {
                            result.Recommendation += $" Note: Pursuing all {results.Count} goals simultaneously requires ${totalRequired:F2}/month but you only have ${availableSurplus:F2} available.";
                        }
                    }
                }
            }
            
            return results;
        }

        /// <summary>
        /// Calculates the number of complete months between now and the deadline
        /// </summary>
        private int CalculateMonthsRemaining(DateTime deadline)
        {
            var now = DateTime.Now;
            
            // If deadline has already passed, return 0
            if (deadline <= now)
            {
                return 0;
            }
            
            // Formula: Months = (Years difference * 12) + Months difference
            var months = ((deadline.Year - now.Year) * 12) + (deadline.Month - now.Month);
            
            // Adjust if we haven't reached the day of the month yet
            if (deadline.Day < now.Day)
            {
                months--;
            }
            
            return Math.Max(0, months);
        }
    }
}
