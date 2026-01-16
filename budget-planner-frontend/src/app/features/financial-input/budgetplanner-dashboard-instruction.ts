GitHub Copilot Agent Prompt – Budget Planner Dashboard

CONTEXT
You are generating code for a Dashboard component in a Budget Planner Web Application.

Frontend: Angular
Backend: ASP.NET Core (.NET) with C#
Database: SQLite
Architecture: Clean architecture (Controller → Service → Database)
All calculations and business logic must be handled on the backend only.

A reference dashboard UI image is provided for layout inspiration.

OBJECTIVE
Build a single-page dashboard UI that displays all required financial information on one screen without any vertical scrolling.

IMPORTANT UI CONSTRAINT
- The entire dashboard must fit within one viewport height
- Use responsive grid layouts
- Use compact cards
- Do NOT require the user to scroll vertically

DASHBOARD UI REQUIREMENTS (ANGULAR)

GLOBAL LAYOUT RULE
- Use a grid-based layout
- All components must be visible simultaneously
- Charts must be compact and optimized for height

LEFT COLUMN – DEBIT CARD DISPLAY
- Card-style UI component
- Display Wells Fargo debit card image
- Masked card number (**** **** **** 6857)
- Card holder name
- Expiry date
- Static visual card data is acceptable

TOP-RIGHT – INCOME CARD
- Card placed to the right of debit card
- Title: Income
- Display total income
- No graphs
- Data must come from backend

MIDDLE ROW – BALANCE & EXPENSES
Available Balance Card:
- Title: Available Balance
- Display backend-calculated balance

Expenses Card:
- Title: Expenses
- Display backend-calculated total expenses

BOTTOM SECTION – EXPENSE BAR GRAPH
- Compact bar graph
- Y-axis: Expense amount
- X-axis: Month
- Bars represent monthly expenses
- Bars change visual style based on expense level:
  LOW, MEDIUM, HIGH

FRONTEND DATA FLOW
- Dashboard component must call backend GET endpoint on ngOnInit
- No financial calculations in frontend
- Frontend only binds backend data

BACKEND REQUIREMENTS

Create GET endpoint:
GET /api/analysis/dashboard

This endpoint must:
- Call AnalysisService
- Return ExpenseDTO or the expense data that is saved in the database

AnalysisService Responsibilities:
- Read data saved via POST /api/analysis/input
- Calculate income, expenses, balance
- Aggregate monthly expenses
- Determine expense level per month
- Return DTO for frontend
Total expense in a particular month for graphs will be considered high if it is above 50% of the total monthly income/income, less than 20% of income would be considered low and above 30% medium, you should use this for the graphs.

STRICT RULES
- No scrolling
- No frontend calculations
- No hardcoded values
- Backend is the source of truth

EXPECTED OUTPUT
- Angular dashboard component (TS, HTML, CSS)
- Backend controller endpoint
- AnalysisService method
- DTO classes
- Proper API integration

