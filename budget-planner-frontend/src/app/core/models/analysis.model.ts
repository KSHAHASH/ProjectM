export interface ExpenseDto {
  category: string;
  amount: number;
  type: string;
}

export interface FinancialHealthDto {
  totalIncome: number;
  totalExpenses: number;
  savingsAmount: number;
  savingsRate: number;
  healthScore: number;
  status: string;
}

export interface BudgetAdherenceDto {
  actualSpending: number;
  budgetLimit: number;
  variance: number;
  adherencePercentage: number;
  status: string;
}

export interface SpendingBehaviorDto {
  totalSpending: number;
  averageTransactionSize: number;
  categoryBreakdown: { [key: string]: number };
  typeDistribution: { [key: string]: number };
  dominantCategory: string;
  insights: string[];
}
