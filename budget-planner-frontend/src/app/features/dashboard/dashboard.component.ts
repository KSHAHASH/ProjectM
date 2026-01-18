import { Component, OnInit } from '@angular/core';
import { AnalysisService } from '../../core/services/analysis.service';
import {
  DashboardDto,
  MonthlyExpenseDto,
  ExpenseLevel,
} from '../../core/models/analysis.model';

interface ChartData {
  name: string;
  value: number;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  dashboardData: DashboardDto | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  // Card information (static for display)
  cardNumber = '**** **** **** 6857';
  cardHolder = 'Ian Kelley';
  expiryDate = '04/24';

  // Chart data
  chartData: ChartData[] = [];

  // Chart options
  view: [number, number] = [700, 300];
  showXAxis = true;
  showYAxis = true;
  gradient = false;
  showLegend = false;
  showXAxisLabel = true;
  xAxisLabel = 'Month';
  showYAxisLabel = true;
  yAxisLabel = 'Expenses ($)';
  colorScheme = {
    domain: [] as string[],
  };

  constructor(private analysisService: AnalysisService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  /**
   * Load dashboard data from backend
   */
  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.analysisService.getDashboard().subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.prepareChartData(data);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
        this.errorMessage = 'Failed to load dashboard data. Please try again.';
        this.isLoading = false;
      },
    });
  }

  /**
   * Prepare data for ngx-charts format
   */
  prepareChartData(data: DashboardDto): void {
    // Prepare chart data with colors embedded
    this.chartData = data.monthlyExpenses.map((expense) => ({
      name: expense.month,
      value: expense.amount,
    }));

    // Create custom color scheme based on expense levels
    const colors: string[] = [];
    data.monthlyExpenses.forEach((expense) => {
      console.log(
        'Expense:',
        expense.month,
        'Level:',
        expense.level,
        'Type:',
        typeof expense.level
      );

      // Handle both string and numeric enum values
      const levelValue: string | number = expense.level;
      const levelString = String(levelValue).toLowerCase();

      if (
        levelValue === 0 ||
        levelValue === ExpenseLevel.Low ||
        levelString === 'low'
      ) {
        colors.push('#10b981'); // Green - Low expense
      } else if (
        levelValue === 1 ||
        levelValue === ExpenseLevel.Medium ||
        levelString === 'medium'
      ) {
        colors.push('#f59e0b'); // Orange - Medium expense
      } else if (
        levelValue === 2 ||
        levelValue === ExpenseLevel.High ||
        levelString === 'high'
      ) {
        colors.push('#ef4444'); // Red - High expense
      } else {
        console.warn(
          'Unknown expense level:',
          expense.level,
          'for month:',
          expense.month
        );
        colors.push('#6b7280'); // Gray - Unknown
      }
    });

    console.log('Color scheme:', colors);

    // Update color scheme
    this.colorScheme = {
      domain: colors,
    };
  }
}
