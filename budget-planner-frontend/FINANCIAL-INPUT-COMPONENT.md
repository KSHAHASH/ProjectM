# Financial Input Component

## Overview

The `FinancialInputComponent` is a modular, user-friendly Angular component that allows users to input their financial data (income and expenses), which is then submitted to the backend for analysis.

## Features

✅ **Income Input**: Enter total monthly income  
✅ **Multiple Expenses**: Add/remove multiple expense entries dynamically  
✅ **Real-time Calculations**: See total income, expenses, and savings update live  
✅ **Category Selection**: Choose from 11 expense categories  
✅ **Date Picker**: Select individual dates for each expense  
✅ **Form Validation**: Required fields and minimum value validation  
✅ **Responsive Design**: Works on desktop, tablet, and mobile devices  
✅ **Results Display**: Shows financial health analysis after submission  
✅ **Clean UI**: Card-based layout with gradient headers

## Usage

### 1. Add to Module

Add the component to your Angular module:

```typescript
import { FinancialInputComponent } from "./features/financial-input/financial-input.component";

@NgModule({
  declarations: [FinancialInputComponent],
  // ...
})
export class AppModule {}
```

### 2. Add Route

Add a route in your routing module:

```typescript
{
  path: 'financial-input',
  component: FinancialInputComponent
}
```

### 3. Use in Template

Simply add the component selector:

```html
<app-financial-input></app-financial-input>
```

## Component Structure

```
financial-input/
├── financial-input.component.ts    # Component logic
├── financial-input.component.html  # Template
└── financial-input.component.css   # Styles
```

## API Integration

The component submits data to:

**Endpoint**: `POST /api/analysis/input?income={income}`  
**Body**: Array of ExpenseDto objects

```json
[
  {
    "category": 0,
    "amount": 1200,
    "date": "2026-01-13"
  }
]
```

**Response**: FinancialHealthDto

```json
{
  "totalIncome": 5000,
  "totalExpenses": 2350,
  "savingsAmount": 2650,
  "savingsRate": 53.0,
  "healthStatus": "Fair",
  "recommendation": "Your finances need attention..."
}
```

## Expense Categories

| Value | Label          |
| ----- | -------------- |
| 0     | Housing        |
| 1     | Transportation |
| 2     | Food           |
| 3     | Utilities      |
| 4     | Healthcare     |
| 5     | Entertainment  |
| 6     | Shopping       |
| 7     | Education      |
| 8     | Insurance      |
| 9     | Savings        |
| 10    | Other          |

## Validation Rules

- **Income**: Required, must be > 0
- **Category**: Required for each expense
- **Amount**: Required, must be > 0
- **Date**: Required for each expense
- **Minimum Expenses**: At least 1 expense required

## Key Methods

### `addExpense()`

Adds a new expense entry to the form

### `removeExpense(index: number)`

Removes an expense entry (minimum 1 required)

### `onSubmit()`

Validates and submits financial data to backend

### `resetForm()`

Clears all data and starts fresh

## Computed Properties

### `totalExpenses`

Sum of all expense amounts

### `totalSavings`

Income minus total expenses (can be negative)

### `income`

Current income value from form

## Styling

The component uses:

- Gradient purple header card
- White form cards with subtle shadows
- Responsive grid layout
- Color-coded health status results
- Mobile-friendly table/list view
- Smooth transitions and hover effects

## Responsive Breakpoints

- **Desktop**: Table view for expenses
- **Tablet**: Grid layout adjusts
- **Mobile (< 768px)**: List view for expenses, stacked layout

## Error Handling

- Form validation errors displayed inline
- API errors shown in alert banner
- Loading state during submission
- Success message after submission

## Example Use Cases

1. **Personal Budget Tracker**: Users enter monthly financial data
2. **Financial Dashboard**: Part of larger financial management app
3. **Expense Analysis Tool**: Quick expense categorization and analysis
4. **Budget Planning**: Plan and analyze potential budgets

## Customization

### Change Colors

Edit CSS variables in `financial-input.component.css`:

```css
.summary-card {
  background: linear-gradient(135deg, #yourColor1 0%, #yourColor2 100%);
}
```

### Add More Categories

Update the `expenseCategories` array in the component:

```typescript
expenseCategories = [
  { value: 11, label: "Your New Category" },
  // ...
];
```

### Modify Validation

Change validators in the FormBuilder:

```typescript
income: [null, [Validators.required, Validators.min(100)]], // Minimum $100
```

## Dependencies

- `@angular/forms` - Reactive Forms
- `@angular/common` - Common pipes and directives
- `AnalysisService` - API communication
- `ExpenseDto`, `FinancialHealthDto` - Type definitions

## Future Enhancements

- 📊 Charts/graphs for expense breakdown
- 💾 Save drafts to local storage
- 📤 Export data to CSV/PDF
- 🔄 Load historical data
- 🎯 Budget goal setting
- 📱 Progressive Web App support

## Notes

- All currency values use 2 decimal places
- Dates are in ISO format (YYYY-MM-DD)
- Form starts with 1 expense row by default
- Maximum number of expenses: unlimited
- All data comes from user input (no hardcoded samples)
