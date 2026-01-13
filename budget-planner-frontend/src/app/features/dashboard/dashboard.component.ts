import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { AnalysisService } from '../../core/services/analysis.service';
import {
  FinancialHealthDto,
  BudgetAdherenceDto,
  SpendingBehaviorDto,
  ExpenseDto,
} from '../../core/models/analysis.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  dashboardForm: FormGroup;
  budgetForm: FormGroup;

  financialHealth: FinancialHealthDto | null = null;
  budgetAdherence: BudgetAdherenceDto | null = null;
  spendingBehavior: SpendingBehaviorDto | null = null;

  isLoading = false;
  error: string | null = null;

  expenseCategories = [
    'Housing',
    'Transportation',
    'Food',
    'Utilities',
    'Healthcare',
    'Entertainment',
    'Shopping',
    'Education',
    'Insurance',
    'Savings',
    'Other',
  ];

  expenseTypes = ['Fixed', 'Variable', 'OneTime'];

  constructor(
    private fb: FormBuilder,
    private analysisService: AnalysisService
  ) {
    this.dashboardForm = this.fb.group({
      income: [null, [Validators.required, Validators.min(0)]],
      expenses: this.fb.array([]),
    });

    this.budgetForm = this.fb.group({
      budgetLimit: [null, [Validators.required, Validators.min(0)]],
    });
  }

  ngOnInit(): void {
    this.addExpense();
  }

  get expenses(): FormArray {
    return this.dashboardForm.get('expenses') as FormArray;
  }

  addExpense(): void {
    const expenseGroup = this.fb.group({
      category: ['', Validators.required],
      amount: [null, [Validators.required, Validators.min(0)]],
      type: ['', Validators.required],
    });
    this.expenses.push(expenseGroup);
  }

  removeExpense(index: number): void {
    if (this.expenses.length > 1) {
      this.expenses.removeAt(index);
    }
  }

  onSubmitDashboard(): void {
    if (this.dashboardForm.invalid) {
      this.error = 'Please fill all required fields correctly';
      return;
    }

    this.isLoading = true;
    this.error = null;

    const income = this.dashboardForm.value.income;
    const expenses: ExpenseDto[] = this.dashboardForm.value.expenses;

    this.analysisService.getDashboard(income, expenses).subscribe(
      data => {
        console.log('Financial health data:', data);
        this.financialHealth = data;
        this.isLoading = false;
        this.loadSpendingBehavior(expenses);
      },
      err => {
        this.error = 'Failed to load financial health data';
        this.isLoading = false;
        console.error('Dashboard error:', err);
      },
    );
  }

  loadSpendingBehavior(expenses: ExpenseDto[]): void {
    this.analysisService.getSpendingBehavior(expenses).subscribe(
      data => {
        console.log('Spending behavior data:', data);
        this.spendingBehavior = data;
      },
      err => {
        console.error('Spending behavior error:', err);
      },
    );
  }

  onSubmitBudget(): void {
    if (this.budgetForm.invalid || !this.financialHealth) {
      this.error = 'Please analyze expenses first and set a budget limit';
      return;
    }

    const budgetLimit = this.budgetForm.value.budgetLimit;
    const actualSpending = this.financialHealth.totalExpenses;

    this.analysisService
      .getBudgetAdherence(actualSpending, budgetLimit)
      .subscribe(
        data => {
          this.budgetAdherence = data;
        },
        err => {
          this.error = 'Failed to load budget adherence data';
          console.error('Budget adherence error:', err);
        },
      );
  }

  get totalExpenses(): number {
    return this.expenses.controls.reduce((sum, control) => {
      return sum + (control.value.amount || 0);
    }, 0);
  }

  get expenseRatio(): number {
    if (!this.financialHealth) return 0;
    return (
      (this.financialHealth.totalExpenses / this.financialHealth.totalIncome) *
      100
    );
  }

  getHealthScoreColor(): string {
    if (!this.financialHealth) return 'gray';
    const score = this.financialHealth.healthScore;
    if (score >= 80) return '#4caf50';
    if (score >= 60) return '#ff9800';
    return '#f44336';
  }

  getHealthScoreWidth(): string {
    if (!this.financialHealth) return '0%';
    return `${this.financialHealth.healthScore}%`;
  }

  getSavingsRateWidth(): string {
    if (!this.financialHealth) return '0%';
    return `${Math.min(this.financialHealth.savingsRate, 100)}%`;
  }

  getExpenseRatioWidth(): string {
    return `${Math.min(this.expenseRatio, 100)}%`;
  }

  getCategoryBreakdownArray(): Array<{ category: string; amount: number }> {
    if (!this.spendingBehavior?.categoryBreakdown) return [];
    return Object.entries(this.spendingBehavior.categoryBreakdown).map(
      ([category, amount]) => ({ category, amount })
    );
  }

  getTypeDistributionArray(): Array<{ type: string; amount: number }> {
    if (!this.spendingBehavior?.typeDistribution) return [];
    return Object.entries(this.spendingBehavior.typeDistribution).map(
      ([type, amount]) => ({ type, amount })
    );
  }
}
