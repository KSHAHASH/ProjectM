# Dashboard Testing Guide

## Overview

The dashboard component has been successfully implemented following all requirements:

### Backend Implementation

✅ **DashboardDto.cs** - Created DTO with:

- TotalIncome
- TotalExpenses
- AvailableBalance
- MonthlyExpenses (with ExpenseLevel: Low/Medium/High)

✅ **AnalysisService.GetDashboardDataAsync()** - Calculates:

- User income from database
- Total expenses from all user expenses
- Available balance (income - expenses)
- Monthly expense aggregation with levels:
  - **High**: > 50% of monthly income
  - **Medium**: 30-50% of monthly income
  - **Low**: < 30% of monthly income

✅ **AnalysisController** - New endpoint:

- `GET /api/analysis/dashboard?userId=1`
- Returns DashboardDto

### Frontend Implementation

✅ **Dashboard Component** created with:

- TypeScript logic
- HTML template
- CSS styling (no-scroll constraint)

✅ **Routing** updated:

- Default route now points to `/dashboard`
- Dashboard accessible via sidebar navigation

✅ **UI Components** as per requirements:

- **Left Column**: Wells Fargo debit card display with balance card
- **Top Right**: Income card
- **Middle Row**: Expenses card
- **Bottom Section**: Monthly expense bar chart with color coding

### Features

- **No vertical scrolling** - All content fits in one viewport
- **Responsive grid layout**
- **Color-coded expense bars**:
  - 🟢 Green - Low expenses (< 30% of income)
  - 🟡 Orange - Medium expenses (30-50% of income)
  - 🔴 Red - High expenses (> 50% of income)
- **Real-time data** from backend
- **Loading states** and error handling

## How to Test

### 1. Add Financial Data

Navigate to **Financial Input** page and submit income and expenses to populate the database.

Example data:

```
Income: $5000

Expenses:
- Housing: $1200 (Fixed) - Sep
- Transportation: $400 (Variable) - Sep
- Food: $600 (Variable) - Oct
- Utilities: $200 (Fixed) - Oct
- Entertainment: $300 (Variable) - Nov
- Healthcare: $150 (Fixed) - Nov
```

### 2. View Dashboard

Navigate to **Dashboard** to see:

- Wells Fargo debit card with balance
- Total income display
- Total expenses display
- Monthly expense bar chart with color-coded bars

### 3. API Endpoints

**GET Dashboard Data:**

```bash
curl http://localhost:5293/api/analysis/dashboard?userId=1
```

**POST Financial Data:**

```bash
curl -X POST http://localhost:5293/api/analysis/input \
  -H "Content-Type: application/json" \
  -d '{
    "Income": 5000,
    "Expenses": [
      {"Category": 0, "Amount": 1200, "Type": 0, "Date": "2024-09-15"},
      {"Category": 1, "Amount": 400, "Type": 1, "Date": "2024-09-20"},
      {"Category": 2, "Amount": 600, "Type": 1, "Date": "2024-10-10"},
      {"Category": 3, "Amount": 200, "Type": 0, "Date": "2024-10-15"},
      {"Category": 4, "Amount": 300, "Type": 1, "Date": "2024-11-05"},
      {"Category": 5, "Amount": 150, "Type": 0, "Date": "2024-11-12"}
    ]
  }'
```

## Enums Reference

### ExpenseCategory

```
0 = Housing
1 = Transportation
2 = Food
3 = Utilities
4 = Entertainment
5 = Healthcare
6 = Education
7 = Shopping
8 = Other
```

### ExpenseType

```
0 = Fixed
1 = Variable
2 = Discretionary
```

### ExpenseLevel (Dashboard)

```
0 = Low (< 30% of income)
1 = Medium (30-50% of income)
2 = High (> 50% of income)
```

## Architecture

### Data Flow

```
User → Financial Input Component
  ↓
  POST /api/analysis/input
  ↓
  AnalysisController.SubmitDashboard()
  ↓
  AnalysisService.CalculateAndSaveFinancialHealthAsync()
  ↓
  Database (SQLite)

User → Dashboard Component
  ↓
  GET /api/analysis/dashboard
  ↓
  AnalysisController.GetDashboard()
  ↓
  AnalysisService.GetDashboardDataAsync()
  ↓
  Database (SQLite)
  ↓
  DashboardDto → Angular Component → UI
```

### Clean Architecture Layers

1. **Domain**: Entities (User, Expense), Enums
2. **Application**: Services, DTOs, Interfaces
3. **Infrastructure**: Database context, Migrations
4. **API**: Controllers, Program.cs
5. **Frontend**: Angular components, Services, Models

## URLs

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5293
- **Dashboard**: http://localhost:4200/dashboard
- **Financial Input**: http://localhost:4200/financial-input
