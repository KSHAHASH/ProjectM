import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from './api.service';
import {
  FinancialHealthDto,
  BudgetAdherenceDto,
  SpendingBehaviorDto,
  ExpenseDto,
} from '../models/analysis.model';

@Injectable({
  providedIn: 'root',
})
export class AnalysisService {
  constructor(private apiService: ApiService) {}

  getDashboard(
    income: number,
    expenses: ExpenseDto[]
  ): Observable<FinancialHealthDto> {
    const requestBody = {
      income: income,
      expenses: expenses, // Send full expense objects with category, type, etc.
    };
    return this.apiService
      .post<FinancialHealthDto>('analysis/dashboard', requestBody)
      .pipe(map((response) => this.mapToFinancialHealth(response)));
  }

  getBudgetAdherence(
    actual: number,
    budgetLimit: number
  ): Observable<BudgetAdherenceDto> {
    return this.apiService
      .get<BudgetAdherenceDto>('analysis/budget', {
        actual,
        budgetLimit,
      })
      .pipe(map((response) => this.mapToBudgetAdherence(response)));
  }

  getSpendingBehavior(expenses: ExpenseDto[]): Observable<SpendingBehaviorDto> {
    return this.apiService
      .post<SpendingBehaviorDto>('analysis/behavior', expenses)
      .pipe(map((response) => this.mapToSpendingBehavior(response)));
  }

  /**
   * Submit financial data (income and expenses) to backend
   * Matches the new API endpoint: POST /api/analysis/input?income={income}
   * Body: array of expenses
   */
  submitFinancialData(
    income: number,
    expenses: ExpenseDto[]
  ): Observable<FinancialHealthDto> {
    return this.apiService
      .post<FinancialHealthDto>(`analysis/input?income=${income}`, expenses)
      .pipe(map((response) => this.mapToFinancialHealth(response)));
  }

  private mapToFinancialHealth(data: any): FinancialHealthDto {
    return {
      totalIncome: data.totalIncome,
      totalExpenses: data.totalExpenses,
      savingsAmount: data.savingsAmount,
      savingsRate: data.savingsRate,
      healthScore: data.healthScore,
      healthStatus: data.healthStatus || data.status, // Support both field names
      recommendation: data.recommendation,
      status: data.status || data.healthStatus, // Support both field names
    };
  }

  private mapToBudgetAdherence(data: any): BudgetAdherenceDto {
    return {
      actualSpending: data.actualSpending,
      budgetLimit: data.budgetLimit,
      variance: data.variance,
      adherencePercentage: data.adherencePercentage,
      status: data.status,
    };
  }

  private mapToSpendingBehavior(data: any): SpendingBehaviorDto {
    return {
      totalSpending: data.totalSpending,
      averageTransactionSize: data.averageTransactionSize,
      categoryBreakdown: data.categoryBreakdown,
      typeDistribution: data.typeDistribution,
      dominantCategory: data.dominantCategory,
      insights: data.insights || [],
    };
  }
}
