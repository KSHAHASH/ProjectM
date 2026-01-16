import { ExpenseCategory } from '../enums/ExpenseCategory';
import { ExpenseType } from '../enums/ExpenseType';

export interface ExpenseDto {
  category: ExpenseCategory | number;
  amount: number;
  type?: ExpenseType;
  date?: string; // ISO date string format
}

export interface FinancialHealthDto {
  totalIncome: number;
  totalExpenses: number;
  savingsAmount: number;
  savingsRate: number;
  healthScore?: number;
  healthStatus?: string; // Changed from 'status' to match backend
  recommendation?: string; // Added to match backend
  status?: string; // Keep for backward compatibility
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

export interface DashboardDto {
  totalIncome: number;
  totalExpenses: number;
  availableBalance: number;
  monthlyExpenses: MonthlyExpenseDto[];
}

export interface MonthlyExpenseDto {
  month: string;
  amount: number;
  level: ExpenseLevel;
}

export enum ExpenseLevel {
  Low = 0,
  Medium = 1,
  High = 2,
}
