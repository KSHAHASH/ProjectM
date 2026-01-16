import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { AnalysisService } from '../../core/services/analysis.service';
import { ExpenseDto } from '../../core/models/analysis.model';
import { ExpenseCategory } from '../../core/enums/ExpenseCategory';
import { ExpenseType } from '../../core/enums/ExpenseType';

@Component({
  selector: 'app-financial-input',
  templateUrl: './financial-input.component.html',
  styleUrls: ['./financial-input.component.css'],
})
export class FinancialInputComponent implements OnInit {
  financialForm: FormGroup;

  isLoading = false;
  isSubmitted = false;
  errorMessage: string | null = null;
  currentDate = new Date();

  // Expense categories from enum
  expenseCategories = Object.values(ExpenseCategory).map((category) => ({
    value: category,
    label: category,
  }));

  // Expense types from enum
  expenseTypes = Object.values(ExpenseType).map((type) => ({
    value: type,
    label: type,
  }));

  constructor(
    private fb: FormBuilder,
    private analysisService: AnalysisService
  ) {
    this.financialForm = this.fb.group({
      income: [null, [Validators.required, Validators.min(0.01)]],
      expenses: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    // Start with one expense row
    this.addExpense();
  }

  /**
   * Get the expenses FormArray
   */
  get expenses(): FormArray {
    return this.financialForm.get('expenses') as FormArray;
  }

  /**
   * Get the current income value
   */
  get income(): number {
    return this.financialForm.get('income')?.value || 0;
  }

  /**
   * Calculate total expenses from all expense entries
   */
  get totalExpenses(): number {
    return this.expenses.controls.reduce((sum, control) => {
      const amount = control.get('amount')?.value || 0;
      return sum + Number(amount);
    }, 0);
  }

  /**
   * Calculate savings (income - total expenses)
   */
  get totalSavings(): number {
    return this.income - this.totalExpenses;
  }

  /**
   * Add a new expense entry
   */
  addExpense(): void {
    const expenseGroup = this.fb.group({
      category: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      type: [null, Validators.required], // Add expense type field
      date: [new Date().toISOString().split('T')[0], Validators.required], // Default to today
    });

    this.expenses.push(expenseGroup);
  }

  /**
   * Remove an expense entry by index
   */
  removeExpense(index: number): void {
    if (this.expenses.length > 1) {
      this.expenses.removeAt(index);
    } else {
      this.errorMessage = 'At least one expense is required';
      setTimeout(() => (this.errorMessage = null), 3000);
    }
  }

  /**
   * Get category label by value
   */
  getCategoryLabel(categoryValue: string): string {
    return categoryValue || 'Unknown';
  }

  /**
   * Submit financial data to the backend
   */
  onSubmit(): void {
    // Mark all fields as touched to show validation errors
    this.markFormGroupTouched(this.financialForm);

    if (this.financialForm.invalid) {
      this.errorMessage = 'Please fill all required fields correctly';
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    //expenses is an array of expenseGroup with category, amount, type, date mentioned above in addExpense method
    const income = this.financialForm.value.income;
    const expenses: ExpenseDto[] = this.financialForm.value.expenses.map(
      (exp: any) => ({
        category: exp.category,
        amount: Number(exp.amount),
        type: exp.type,
        date: exp.date,
      })
    );

    console.log('Submitting financial data:', { income, expenses });

    this.analysisService.submitFinancialData(income, expenses).subscribe({
      next: (response) => {
        this.isSubmitted = true;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error submitting financial data:', error);
        console.error('Error details:', JSON.stringify(error.error, null, 2));
        this.errorMessage =
          error.error?.message ||
          error.error ||
          'Failed to submit financial data. Please try again.';
        this.isLoading = false;
      },
    });
  }

  /**
   * Reset the form and start fresh
   */
  resetForm(): void {
    this.financialForm.reset();
    this.expenses.clear();
    this.addExpense();
    this.isSubmitted = false;
    this.errorMessage = null;
  }

  /**
   * Helper to mark all form fields as touched
   */
  private markFormGroupTouched(formGroup: FormGroup | FormArray): void {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup || control instanceof FormArray) {
        this.markFormGroupTouched(control);
      }
    });
  }

  /**
   * Check if a form control has an error
   */
  hasError(controlName: string, errorType: string): boolean {
    const control = this.financialForm.get(controlName);
    return !!(
      control &&
      control.hasError(errorType) &&
      (control.dirty || control.touched)
    );
  }

  /**
   * Check if an expense control has an error
   */
  hasExpenseError(
    index: number,
    controlName: string,
    errorType: string
  ): boolean {
    const control = this.expenses.at(index).get(controlName);
    return !!(
      control &&
      control.hasError(errorType) &&
      (control.dirty || control.touched)
    );
  }
}
