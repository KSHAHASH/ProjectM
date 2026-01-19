import { Component, OnInit } from '@angular/core';
import { MonthlyAnalysisService } from '../../../core/services/monthly-analysis.service';
import {
  MonthlyAnalysis,
  ExpenseCategoryBreakdown,
} from '../../../core/models/monthly-analysis.model';

@Component({
  selector: 'app-monthly-analysis',
  templateUrl: './monthly-analysis.component.html',
  styleUrls: ['./monthly-analysis.component.css'],
})
export class MonthlyAnalysisComponent implements OnInit {
  selectedMonth: number = new Date().getMonth() + 1; // Current month (1-12)
  selectedYear: number = new Date().getFullYear();

  monthlyData: MonthlyAnalysis | null = null;
  loading: boolean = false;
  error: string | null = null;

  // Month options for dropdown
  months = [
    { value: 1, name: 'January' },
    { value: 2, name: 'February' },
    { value: 3, name: 'March' },
    { value: 4, name: 'April' },
    { value: 5, name: 'May' },
    { value: 6, name: 'June' },
    { value: 7, name: 'July' },
    { value: 8, name: 'August' },
    { value: 9, name: 'September' },
    { value: 10, name: 'October' },
    { value: 11, name: 'November' },
    { value: 12, name: 'December' },
  ];

  // Chart data
  pieChartData: any[] = [];
  colorScheme = {
    domain: [
      '#2196F3',
      '#F44336',
      '#4CAF50',
      '#FF9800',
      '#9C27B0',
      '#00BCD4',
      '#FFEB3B',
      '#795548',
    ],
  };

  constructor(private monthlyAnalysisService: MonthlyAnalysisService) {}

  ngOnInit(): void {
    this.loadMonthlyAnalysis();
  }

  onMonthChange(): void {
    this.loadMonthlyAnalysis();
  }

  loadMonthlyAnalysis(): void {
    this.loading = true;
    this.error = null;

    this.monthlyAnalysisService
      .getMonthlyAnalysis(this.selectedYear, this.selectedMonth)
      .subscribe({
        next: (data) => {
          this.monthlyData = data;
          this.preparePieChartData();
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading monthly analysis:', err);
          this.error = 'Failed to load monthly analysis. Please try again.';
          this.loading = false;
        },
      });
  }

  preparePieChartData(): void {
    if (!this.monthlyData || !this.monthlyData.expenseBreakdown) {
      this.pieChartData = [];
      return;
    }

    this.pieChartData = this.monthlyData.expenseBreakdown.map(
      (item: ExpenseCategoryBreakdown) => ({
        name: item.category,
        value: item.amount,
      })
    );
  }

  getSelectedMonthName(): string {
    const month = this.months.find((m) => m.value === this.selectedMonth);
    return month ? month.name : '';
  }

  getRecommendationIcon(icon: string): string {
    switch (icon) {
      case 'up':
        return '↑';
      case 'down':
        return '↓';
      case 'lightbulb':
        return '💡';
      case 'check':
        return '✓';
      default:
        return '•';
    }
  }

  getRecommendationClass(type: string): string {
    switch (type) {
      case 'warning':
        return 'recommendation-warning';
      case 'success':
        return 'recommendation-success';
      case 'tip':
        return 'recommendation-tip';
      default:
        return 'recommendation-info';
    }
  }

  getChangeIcon(change: number): string {
    if (change > 0) return '↑';
    if (change < 0) return '↓';
    return '';
  }

  getChangeClass(change: number, inverseColors: boolean = false): string {
    if (inverseColors) {
      // For expenses, increase is bad (red), decrease is good (green)
      if (change > 0) return 'change-negative';
      if (change < 0) return 'change-positive';
    } else {
      // For income/savings, increase is good (green), decrease is bad (red)
      if (change > 0) return 'change-positive';
      if (change < 0) return 'change-negative';
    }
    return '';
  }
}
