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
    const expensesParam = expenses.map((e) => e.amount);
    return this.apiService
      .get<FinancialHealthDto>('analysis/dashboard', {
        income,
        expenses: expensesParam,
      })
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

  private mapToFinancialHealth(data: any): FinancialHealthDto {
    return {
      totalIncome: data.totalIncome,
      totalExpenses: data.totalExpenses,
      savingsAmount: data.savingsAmount,
      savingsRate: data.savingsRate,
      healthScore: data.healthScore,
      status: data.status,
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
