import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { AnalysisService } from '../../core/services/analysis.service';
import {
  BudgetAdherenceDto,
  ExpenseDto,
  ExpenseCategory,
} from '../../core/models/analysis.model';

interface CategoryBudget {
  category: string;
  budgetLimit: number;
  actualSpending: number;
  variance: number;
  adherencePercentage: number;
  status: string;
}

@Component({
  selector: 'app-budget-analysis',
  templateUrl: './budget-analysis.component.html',
  styleUrls: ['./budget-analysis.component.css'],
})
export class BudgetAnalysisComponent implements OnInit {
  budgetForm: FormGroup;
  
  categoryBudgets: CategoryBudget[] = [];
  overallBudgetAdherence: BudgetAdherenceDto | null = null;
  
  isLoading = false;
  error: string | null = null;
  
  expenseCategories = Object.values(ExpenseCategory);
  expenseTypes = ['Fixed', 'Variable', 'OneTime'];

  constructor(
    private fb: FormBuilder,
    private analysisService: AnalysisService
  ) {
    this.budgetForm = this.fb.group({
      expenses: this.fb.array([]),
      categoryBudgets: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    this.addExpense();
    this.addCategoryBudget();
  }

  get expenses(): FormArray {
    return this.budgetForm.get('expenses') as FormArray;
  }

  get budgets(): FormArray {
    return this.budgetForm.get('categoryBudgets') as FormArray;
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

  addCategoryBudget(): void {
    const budgetGroup = this.fb.group({
      category: ['', Validators.required],
      budgetLimit: [null, [Validators.required, Validators.min(0)]],
    });
    this.budgets.push(budgetGroup);
  }

  removeCategoryBudget(index: number): void {
    if (this.budgets.length > 1) {
      this.budgets.removeAt(index);
    }
  }

  onSubmit(): void {
    if (this.budgetForm.invalid) {
      this.error = 'Please fill all required fields correctly';
      return;
    }

    this.isLoading = true;
    this.error = null;

    const expenses: ExpenseDto[] = this.budgetForm.value.expenses;
    const budgets = this.budgetForm.value.categoryBudgets;

    // Group expenses by category
    const expensesByCategory = this.groupExpensesByCategory(expenses);

    // Calculate budget adherence for each category
    this.categoryBudgets = budgets.map((budget: any) => {
      const actualSpending = expensesByCategory[budget.category] || 0;
      return this.calculateCategoryBudget(
        budget.category,
        actualSpending,
        budget.budgetLimit
      );
    });

    // Calculate overall budget adherence
    const totalActual = expenses.reduce((sum, exp) => sum + exp.amount, 0);
    const totalBudget = budgets.reduce(
      (sum: number, b: any) => sum + b.budgetLimit,
      0
    );

    this.analysisService.getBudgetAdherence(totalActual, totalBudget).subscribe(
      (data) => {
        this.overallBudgetAdherence = data;
        this.isLoading = false;
      },
      (err) => {
        this.error = 'Failed to load budget adherence data';
        this.isLoading = false;
        console.error('Budget analysis error:', err);
      }
    );
  }

  private groupExpensesByCategory(
    expenses: ExpenseDto[]
  ): { [key: string]: number } {
    return expenses.reduce((acc, expense) => {
      const category = expense.category;
      acc[category] = (acc[category] || 0) + expense.amount;
      return acc;
    }, {} as { [key: string]: number });
  }

  private calculateCategoryBudget(
    category: string,
    actual: number,
    budgetLimit: number
  ): CategoryBudget {
    const variance = budgetLimit - actual;
    const adherencePercentage = budgetLimit > 0 ? (actual / budgetLimit) * 100 : 0;
    
    let status: string;
    if (adherencePercentage <= 80) {
      status = 'Excellent';
    } else if (adherencePercentage <= 95) {
      status = 'Good';
    } else if (adherencePercentage <= 100) {
      status = 'On Track';
    } else if (adherencePercentage <= 110) {
      status = 'Over Budget';
    } else {
      status = 'Critical';
    }

    return {
      category,
      budgetLimit,
      actualSpending: actual,
      variance,
      adherencePercentage: Math.round(adherencePercentage),
      status,
    };
  }

  get totalBudget(): number {
    return this.budgets.controls.reduce((sum, control) => {
      return sum + (control.value.budgetLimit || 0);
    }, 0);
  }

  get totalActual(): number {
    return this.expenses.controls.reduce((sum, control) => {
      return sum + (control.value.amount || 0);
    }, 0);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Excellent':
      case 'Good':
        return '#4caf50';
      case 'On Track':
        return '#2196f3';
      case 'Over Budget':
        return '#ff9800';
      case 'Critical':
        return '#f44336';
      default:
        return '#9e9e9e';
    }
  }

  getAdherenceBarWidth(percentage: number): string {
    return `${Math.min(percentage, 100)}%`;
  }

  getAdherenceBarColor(percentage: number): string {
    if (percentage <= 80) return '#4caf50';
    if (percentage <= 95) return '#8bc34a';
    if (percentage <= 100) return '#2196f3';
    if (percentage <= 110) return '#ff9800';
    return '#f44336';
  }
}
