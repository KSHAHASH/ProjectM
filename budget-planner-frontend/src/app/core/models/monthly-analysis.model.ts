export interface MonthlyAnalysis {
  totalIncome: number;
  totalExpenses: number;
  totalSavings: number;
  savingsRate: number;
  incomeChange: number;
  expenseChange: number;
  savingsChange: number;
  savingsRateChange: number;
  expenseBreakdown: ExpenseCategoryBreakdown[];
  recommendations: Recommendation[];
  hasSufficientData: boolean;
}

export interface ExpenseCategoryBreakdown {
  category: string;
  amount: number;
  percentage: number;
}

export interface Recommendation {
  type: string; // "warning", "success", "tip", "info"
  icon: string; // "up", "down", "lightbulb", "check"
  title: string;
  message: string;
  highlightedValue: string;
}
