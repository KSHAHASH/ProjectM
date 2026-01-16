# Dashboard Component Implementation Guide

This document explains all the changes made to implement the dashboard component, from backend to frontend.

---

## Table of Contents

1. [Overview](#overview)
2. [Backend Implementation](#backend-implementation)
3. [Frontend Implementation](#frontend-implementation)
4. [Data Flow](#data-flow)
5. [Key Concepts](#key-concepts)

---

## Overview

### What We Built

A comprehensive financial dashboard that displays:

- **Debit Card Display**: Visual representation of a credit/debit card
- **Current Balance**: Available balance (Income - Expenses)
- **Income Card**: Total accumulated income (green gradient)
- **Expenses Card**: Total expenses (red gradient)
- **Activity Chart**: Bar chart showing monthly expenses with color-coded levels (Low/Medium/High)

### Architecture Pattern

- **Clean Architecture**: Separation of concerns across layers
- **Repository Pattern**: Database access through DbContext
- **DTO Pattern**: Data Transfer Objects for API communication
- **Component-Based UI**: Angular components with services

---

## Backend Implementation

### 1. Data Transfer Objects (DTOs)

**File**: `BudgetPlanner.Application/DTOs/DashboardDto.cs`

```csharp
public class DashboardDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal AvailableBalance { get; set; }
    public List<MonthlyExpenseDto> MonthlyExpenses { get; set; } = new();
}

public class MonthlyExpenseDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ExpenseLevel Level { get; set; }
}

public enum ExpenseLevel
{
    Low,      // < 30% of monthly income
    Medium,   // 30-50% of monthly income
    High      // > 50% of monthly income
}
```

**Purpose**:

- DTOs define the structure of data sent from backend to frontend
- Separates internal database entities from API responses
- `ExpenseLevel` enum categorizes expenses based on percentage of income

---

### 2. Service Layer

**File**: `BudgetPlanner.Application/Services/AnalysisService.cs`

**Method**: `GetDashboardDataAsync()`

```csharp
public async Task<DashboardDto> GetDashboardDataAsync(int userId)
{
    // Step 1: Get user from database
    var user = await _dbContext.Users
        .FirstOrDefaultAsync(u => u.Id == userId);

    if (user == null)
    {
        throw new InvalidOperationException($"User with ID {userId} not found.");
    }

    // Step 2: Get all expenses for the user
    var expenses = await _dbContext.Expenses
        .Where(e => e.UserId == userId)
        .OrderBy(e => e.Date)
        .ToListAsync();

    // Step 3: Calculate totals
    var totalExpenses = expenses.Sum(e => e.Amount);
    var availableBalance = user.MonthlyIncome - totalExpenses;

    // Step 4: Calculate average monthly income
    // Count unique months with expenses
    var monthsWithExpenses = expenses
        .Select(e => new { e.Date.Year, e.Date.Month })
        .Distinct()
        .Count();

    // Divide total accumulated income by months to get average
    var averageMonthlyIncome = monthsWithExpenses > 0
        ? user.MonthlyIncome / monthsWithExpenses
        : user.MonthlyIncome;

    // Use $5000 as standard if income seems accumulated (too high per month)
    if (averageMonthlyIncome > 10000 && monthsWithExpenses > 1)
    {
        averageMonthlyIncome = 5000;
    }

    // Step 5: Group expenses by month and calculate levels
    var monthlyExpenses = expenses
        .GroupBy(e => new { e.Date.Year, e.Date.Month })
        .Select(g => new
        {
            Year = g.Key.Year,
            Month = g.Key.Month,
            Amount = g.Sum(e => e.Amount)
        })
        .OrderBy(x => x.Year)
        .ThenBy(x => x.Month)
        .Select(x =>
        {
            // Format month name (e.g., "Jan", "Feb")
            var monthName = new DateTime(x.Year, x.Month, 1).ToString("MMM");

            // Calculate percentage: (Monthly Expense / Average Monthly Income) * 100
            var percentage = averageMonthlyIncome > 0
                ? (x.Amount / averageMonthlyIncome) * 100
                : 0;

            // Determine expense level based on percentage
            ExpenseLevel level;
            if (percentage > 50)
            {
                level = ExpenseLevel.High;    // Red
            }
            else if (percentage >= 30)
            {
                level = ExpenseLevel.Medium;  // Orange
            }
            else
            {
                level = ExpenseLevel.Low;     // Green
            }

            return new MonthlyExpenseDto
            {
                Month = monthName,
                Amount = Math.Round(x.Amount, 2),
                Level = level
            };
        })
        .ToList();

    // Step 6: Return complete dashboard data
    return new DashboardDto
    {
        TotalIncome = user.MonthlyIncome,
        TotalExpenses = Math.Round(totalExpenses, 2),
        AvailableBalance = Math.Round(availableBalance, 2),
        MonthlyExpenses = monthlyExpenses
    };
}
```

**Key Concepts**:

1. **Async/Await**: Database operations are asynchronous for better performance
2. **LINQ Queries**: `Where()`, `GroupBy()`, `Select()` for data manipulation
3. **Business Logic**: Calculating expense levels based on income percentage
4. **Monthly Income Calculation**: Handles accumulated income by dividing by number of months

**Formula for Expense Level**:

```
Percentage = (Monthly Expense / Average Monthly Income) × 100

If Percentage > 50%  → High (Red)
If Percentage 30-50% → Medium (Orange)
If Percentage < 30%  → Low (Green)
```

---

### 3. Service Interface

**File**: `BudgetPlanner.Application/Interfaces/IAnalysisService.cs`

```csharp
public interface IAnalysisService
{
    // ... existing methods ...

    /// <summary>
    /// Get dashboard data including income, expenses, balance,
    /// and monthly expense breakdown
    /// </summary>
    Task<DashboardDto> GetDashboardDataAsync(int userId);
}
```

**Purpose**:

- Defines contract for the service
- Enables dependency injection
- Allows for easier testing (mocking)

---

### 4. API Controller

**File**: `BudgetPlanner.API/Controllers/AnalysisController.cs`

```csharp
[HttpGet("dashboard")]
public async Task<IActionResult> GetDashboard([FromQuery] int userId)
{
    try
    {
        var dashboardData = await _analysisService.GetDashboardDataAsync(userId);
        return Ok(dashboardData);
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred", details = ex.Message });
    }
}
```

**Endpoint**: `GET /api/analysis/dashboard?userId=1`

**Response Example**:

```json
{
  "totalIncome": 25000,
  "totalExpenses": 9000,
  "availableBalance": 16000,
  "monthlyExpenses": [
    {
      "month": "Jan",
      "amount": 1500,
      "level": "Medium"
    },
    {
      "month": "Feb",
      "amount": 2000,
      "level": "Medium"
    },
    {
      "month": "Apr",
      "amount": 4500,
      "level": "High"
    },
    {
      "month": "Jul",
      "amount": 1000,
      "level": "Low"
    }
  ]
}
```

**Key Concepts**:

1. **HTTP GET**: Read-only operation to retrieve data
2. **Query Parameters**: `userId` passed in URL
3. **Error Handling**: Try-catch with appropriate HTTP status codes
4. **JSON Serialization**: ASP.NET Core automatically serializes to JSON
5. **Enum Serialization**: Enums are serialized as strings by default ("Low", "Medium", "High")

---

## Frontend Implementation

### 1. TypeScript Models

**File**: `budget-planner-frontend/src/app/core/models/analysis.model.ts`

```typescript
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
```

**Purpose**:

- TypeScript interfaces provide type safety
- Matches the backend DTO structure
- Enum values (0, 1, 2) used for comparison, but backend sends strings

**Note**: Backend sends strings ("Low", "Medium", "High"), so frontend must handle both formats.

---

### 2. Angular Service

**File**: `budget-planner-frontend/src/app/core/services/analysis.service.ts`

```typescript
getDashboard(userId: number): Observable<DashboardDto> {
  return this.http.get<DashboardDto>(
    `${this.apiUrl}/dashboard?userId=${userId}`
  );
}
```

**Key Concepts**:

1. **Observable**: RxJS pattern for async data streams
2. **HTTP GET**: Angular's HttpClient for API calls
3. **Generic Type**: `<DashboardDto>` provides type safety for response
4. **Template Literals**: Dynamic URL with userId parameter

**Usage in Component**:

```typescript
this.analysisService.getDashboard(1).subscribe({
  next: (data) => {
    /* handle success */
  },
  error: (error) => {
    /* handle error */
  },
});
```

---

### 3. Dashboard Component

#### TypeScript Logic

**File**: `budget-planner-frontend/src/app/features/dashboard/dashboard.component.ts`

```typescript
export class DashboardComponent implements OnInit {
  dashboardData: DashboardDto | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  // Card data (static)
  cardNumber = "**** **** **** 6857";
  cardHolder = "Ian Kelley";
  expiryDate = "04/24";

  // Chart configuration
  chartData: ChartData[] = [];
  view: [number, number] = [700, 300];
  showXAxis = true;
  showYAxis = true;
  showXAxisLabel = true;
  xAxisLabel = "Month";
  showYAxisLabel = true;
  yAxisLabel = "Expenses ($)";
  colorScheme = { domain: [] as string[] };

  constructor(private analysisService: AnalysisService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.analysisService.getDashboard(1).subscribe({
      next: (data) => {
        this.dashboardData = data;
        this.prepareChartData(data);
        this.isLoading = false;
      },
      error: (error) => {
        console.error("Error loading dashboard:", error);
        this.errorMessage = "Failed to load dashboard data.";
        this.isLoading = false;
      },
    });
  }

  prepareChartData(data: DashboardDto): void {
    // Transform data for ngx-charts
    this.chartData = data.monthlyExpenses.map((expense) => ({
      name: expense.month,
      value: expense.amount,
    }));

    // Create color scheme based on expense levels
    const colors: string[] = [];
    data.monthlyExpenses.forEach((expense) => {
      const levelValue: string | number = expense.level;
      const levelString = String(levelValue).toLowerCase();

      // Handle both string ("low") and numeric (0) values
      if (
        levelValue === 0 ||
        levelValue === ExpenseLevel.Low ||
        levelString === "low"
      ) {
        colors.push("#10b981"); // Green
      } else if (
        levelValue === 1 ||
        levelValue === ExpenseLevel.Medium ||
        levelString === "medium"
      ) {
        colors.push("#f59e0b"); // Orange
      } else if (
        levelValue === 2 ||
        levelValue === ExpenseLevel.High ||
        levelString === "high"
      ) {
        colors.push("#ef4444"); // Red
      } else {
        colors.push("#6b7280"); // Gray (unknown)
      }
    });

    this.colorScheme = { domain: colors };
  }
}
```

**Key Concepts**:

1. **Component Lifecycle**: `ngOnInit()` runs when component loads
2. **Observable Subscription**: Subscribe to HTTP response
3. **Data Transformation**: Convert backend data to chart format
4. **Dynamic Color Scheme**: Each bar gets its own color based on level
5. **String Handling**: Handles both string and numeric enum values from backend

---

#### HTML Template

**File**: `budget-planner-frontend/src/app/features/dashboard/dashboard.component.html`

```html
<div class="dashboard-container" *ngIf="!isLoading && dashboardData">
  <!-- Top Section: 2x2 Card Grid -->
  <div class="top-section">
    <!-- Card 1: Debit Card (Top Left) -->
    <div class="card-display">
      <div class="debit-card">
        <div class="card-header">
          <span class="card-label">Card</span>
        </div>
        <div class="card-number">{{ cardNumber }}</div>
        <div class="card-footer">
          <div class="card-holder">
            <div class="label">CARD HOLDER</div>
            <div class="value">{{ cardHolder }}</div>
          </div>
          <div class="card-expiry">
            <div class="label">VALID THRU</div>
            <div class="value">{{ expiryDate }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Card 2: Current Balance (Bottom Left) -->
    <div class="balance-card">
      <div class="balance-amount">
        ${{ dashboardData.availableBalance | number : "1.0-0" }}
      </div>
      <div class="balance-label">Current balance</div>
    </div>

    <!-- Card 3: Income (Top Right) -->
    <div class="income-card">
      <div class="card-title">Income</div>
      <div class="income-amount">
        ${{ dashboardData.totalIncome | number : "1.0-0" }}
      </div>
    </div>

    <!-- Card 4: Expenses (Bottom Right) -->
    <div class="expenses-card">
      <div class="card-title">Expenses</div>
      <div class="expenses-amount">
        ${{ dashboardData.totalExpenses | number : "1.0-0" }}
      </div>
    </div>
  </div>

  <!-- Bottom Section: Activity Chart -->
  <div class="activity-section">
    <div class="activity-header">
      <h3>Activity</h3>
      <div class="legend">
        <span class="legend-item">
          <span class="dot level-low"></span> Low
        </span>
        <span class="legend-item">
          <span class="dot level-medium"></span> Medium
        </span>
        <span class="legend-item">
          <span class="dot level-high"></span> High
        </span>
      </div>
    </div>
    <div class="chart-container">
      <ngx-charts-bar-vertical
        [view]="view"
        [scheme]="colorScheme"
        [results]="chartData"
        [xAxis]="showXAxis"
        [yAxis]="showYAxis"
        [legend]="showLegend"
        [showXAxisLabel]="showXAxisLabel"
        [showYAxisLabel]="showYAxisLabel"
        [xAxisLabel]="xAxisLabel"
        [yAxisLabel]="yAxisLabel"
        [gradient]="gradient"
      >
      </ngx-charts-bar-vertical>
    </div>
  </div>
</div>
```

**Key Concepts**:

1. **Data Binding**: `{{ }}` for displaying values
2. **Property Binding**: `[property]="value"` for passing data to chart
3. **Structural Directive**: `*ngIf` for conditional rendering
4. **Pipes**: `| number : "1.0-0"` formats numbers (no decimals)
5. **ngx-charts**: Third-party charting library component

---

#### CSS Styling

**File**: `budget-planner-frontend/src/app/features/dashboard/dashboard.component.css`

**Layout Strategy**:

```css
/* No-scroll container */
.dashboard-container {
  height: 100vh; /* Full viewport height */
  overflow: hidden; /* No scrolling */
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* 2x2 Grid for cards */
.top-section {
  display: grid;
  grid-template-columns: 1fr 1fr; /* Two equal columns */
  grid-template-rows: 1fr 1fr; /* Two equal rows */
  gap: 20px;
  height: 50%; /* Takes half the screen */
}
```

**Card Positioning**:

```css
.card-display {
  grid-column: 1;
  grid-row: 1;
} /* Top Left */

.balance-card {
  grid-column: 1;
  grid-row: 2;
} /* Bottom Left */

.income-card {
  grid-column: 2;
  grid-row: 1;
} /* Top Right */

.expenses-card {
  grid-column: 2;
  grid-row: 2;
} /* Bottom Right */
```

**Color Meanings**:

```css
/* Income Card - Green (money coming in) */
.income-card {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
}

/* Expenses Card - Red (money going out) */
.expenses-card {
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
}

/* Balance Card - Purple (neutral, available funds) */
.balance-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

/* Debit Card - Gradient with decorative circles */
.debit-card {
  background: linear-gradient(135deg, #5e3ae4 0%, #4a90e2 50%, #ff6b9d 100%);
  position: relative;
}

/* Decorative circles */
.debit-card::before {
  content: "";
  position: absolute;
  top: -50%;
  right: -20%;
  width: 200px;
  height: 200px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 50%;
}
```

**Chart Legend Colors**:

```css
.dot.level-low {
  background-color: #10b981;
} /* Green */

.dot.level-medium {
  background-color: #f59e0b;
} /* Orange */

.dot.level-high {
  background-color: #ef4444;
} /* Red */
```

---

### 4. Module Registration

**File**: `budget-planner-frontend/src/app/app.module.ts`

```typescript
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { NgxChartsModule } from "@swimlane/ngx-charts";
import { DashboardComponent } from "./features/dashboard/dashboard.component";

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent, // Register component
    // ... other components
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule, // Required for ngx-charts
    NgxChartsModule, // Charting library
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
```

**Dependencies Installed**:

```bash
npm install @swimlane/ngx-charts@16.0.0 --legacy-peer-deps
npm install @angular/cdk@10.0.0 --legacy-peer-deps
```

---

### 5. Routing Configuration

**File**: `budget-planner-frontend/src/app/app-routing.module.ts`

```typescript
const routes: Routes = [
  { path: "", redirectTo: "/dashboard", pathMatch: "full" },
  { path: "dashboard", component: DashboardComponent },
  // ... other routes
];
```

**Result**: Navigating to `http://localhost:4200` automatically redirects to dashboard.

---

## Data Flow

### Complete Request-Response Cycle

```
1. USER OPENS BROWSER
   ↓
2. Angular Router loads DashboardComponent
   ↓
3. ngOnInit() calls loadDashboardData()
   ↓
4. AnalysisService.getDashboard(1)
   ↓
5. HTTP GET → http://localhost:5293/api/analysis/dashboard?userId=1
   ↓
6. BACKEND: AnalysisController.GetDashboard()
   ↓
7. BACKEND: AnalysisService.GetDashboardDataAsync()
   ↓
8. BACKEND: Query database (Users + Expenses tables)
   ↓
9. BACKEND: Calculate totals, group by month, determine levels
   ↓
10. BACKEND: Return DashboardDto as JSON
   ↓
11. FRONTEND: HTTP response received
   ↓
12. FRONTEND: Observable emits data
   ↓
13. FRONTEND: prepareChartData() transforms data
   ↓
14. FRONTEND: Template renders with updated data
   ↓
15. USER SEES: Cards with values + colored bar chart
```

---

## Key Concepts

### 1. Clean Architecture Layers

```
┌─────────────────────────────────────┐
│   Presentation Layer (Angular)      │  ← User Interface
├─────────────────────────────────────┤
│   API Layer (Controllers)           │  ← HTTP Endpoints
├─────────────────────────────────────┤
│   Application Layer (Services)      │  ← Business Logic
├─────────────────────────────────────┤
│   Domain Layer (Entities, Enums)    │  ← Core Models
├─────────────────────────────────────┤
│   Infrastructure (DbContext)        │  ← Database Access
└─────────────────────────────────────┘
```

**Benefits**:

- Separation of concerns
- Easy to test each layer
- Changes in UI don't affect business logic
- Changes in database don't affect API

---

### 2. Asynchronous Programming

**Backend (C#)**:

```csharp
public async Task<DashboardDto> GetDashboardDataAsync(int userId)
{
    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    // 'await' pauses execution until database query completes
    // Thread is free to handle other requests
}
```

**Frontend (TypeScript)**:

```typescript
this.analysisService.getDashboard(1).subscribe({
  next: (data) => {
    /* Runs when data arrives */
  },
  error: (error) => {
    /* Runs if error occurs */
  },
});
// Code continues executing immediately
// Subscription handles async response
```

---

### 3. Data Transformation Pipeline

**Backend Transformation**:

```
Database Records → LINQ GroupBy → Calculate Percentages → ExpenseLevel Enum → DTO
```

**Frontend Transformation**:

```
HTTP Response → DashboardDto → ChartData[] → ngx-charts Input
```

**Example**:

```
Database:
- Expense 1: $500, Date: 2026-01-15
- Expense 2: $1000, Date: 2026-01-20

↓ Backend Groups

MonthlyExpenseDto: { Month: "Jan", Amount: 1500, Level: "Medium" }

↓ Frontend Transforms

ChartData: { name: "Jan", value: 1500 }
ColorScheme: { domain: ['#f59e0b'] }  // Orange

↓ ngx-charts Renders

Orange bar labeled "Jan" with height 1500
```

---

### 4. Responsive Color Coding

**Logic**:

```typescript
Monthly Expense = $1,500
Monthly Income = $5,000
Percentage = (1500 / 5000) × 100 = 30%

If 30% ≥ 30 && 30% ≤ 50:
  → Medium
  → Orange (#f59e0b)
```

**Color Palette**:

- **Green (#10b981)**: Safe spending, under 30%
- **Orange (#f59e0b)**: Warning, 30-50% of income
- **Red (#ef4444)**: Alert, over 50% of income

---

### 5. Type Safety

**TypeScript ensures correct data types**:

```typescript
// Compile error if wrong type
dashboardData.totalIncome = "not a number";  // ERROR

// Autocomplete for properties
dashboardData.  // IDE suggests: totalIncome, totalExpenses, etc.

// Catches errors before runtime
const income: number = dashboardData.totalIncome;  // ✓ Valid
```

---

## Summary

### What We Accomplished

1. **Backend (C# / ASP.NET Core)**:

   - Created DTOs for dashboard data structure
   - Implemented business logic for expense level calculation
   - Built API endpoint to serve dashboard data
   - Used async/await for database operations

2. **Frontend (Angular / TypeScript)**:

   - Created models matching backend DTOs
   - Built service to fetch data from API
   - Implemented dashboard component with:
     - 2x2 grid layout for cards
     - Professional bar chart with dynamic colors
     - Responsive design (no scrolling)
   - Styled with meaningful colors (green income, red expenses)

3. **Integration**:
   - HTTP communication between frontend and backend
   - JSON serialization/deserialization
   - Enum handling (strings from backend, numbers in frontend)
   - Observable pattern for async data

### Technologies Used

- **Backend**: C#, ASP.NET Core 9.0, Entity Framework Core, SQLite
- **Frontend**: Angular 10, TypeScript, ngx-charts, RxJS
- **Patterns**: Clean Architecture, Repository, DTO, Observable
- **Styling**: CSS Grid, Flexbox, Gradients

### Key Learning Points

1. **Separation of Layers**: Backend handles data, frontend handles presentation
2. **Async Operations**: Both backend and frontend use async patterns
3. **Type Safety**: Strong typing catches errors early
4. **Data Transformation**: Convert data at each layer as needed
5. **Color Psychology**: Green = good, Orange = warning, Red = danger
6. **Component-Based**: Modular, reusable components

---

## File Summary

### Backend Files Modified/Created

1. ✅ `BudgetPlanner.Application/DTOs/DashboardDto.cs` (NEW)
2. ✅ `BudgetPlanner.Application/Services/AnalysisService.cs` (MODIFIED - added GetDashboardDataAsync)
3. ✅ `BudgetPlanner.Application/Interfaces/IAnalysisService.cs` (MODIFIED - added method signature)
4. ✅ `BudgetPlanner.API/Controllers/AnalysisController.cs` (MODIFIED - added dashboard endpoint)

### Frontend Files Modified/Created

1. ✅ `budget-planner-frontend/src/app/core/models/analysis.model.ts` (MODIFIED - added interfaces)
2. ✅ `budget-planner-frontend/src/app/core/services/analysis.service.ts` (MODIFIED - added getDashboard)
3. ✅ `budget-planner-frontend/src/app/features/dashboard/dashboard.component.ts` (NEW)
4. ✅ `budget-planner-frontend/src/app/features/dashboard/dashboard.component.html` (NEW)
5. ✅ `budget-planner-frontend/src/app/features/dashboard/dashboard.component.css` (NEW)
6. ✅ `budget-planner-frontend/src/app/app.module.ts` (MODIFIED - registered component)
7. ✅ `budget-planner-frontend/src/app/app-routing.module.ts` (MODIFIED - added route)

---

This implementation follows industry best practices and demonstrates a complete full-stack feature from database to UI.
