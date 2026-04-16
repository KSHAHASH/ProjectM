## 🚀 Run Server

```bash
## Run Server
cd BudgetPlanner.API
dotnet build
dotnet run

## Run Migrations
cd ..
dotnet ef migrations add AddToken --project BudgetPlanner.Infrastructure --startup-project BudgetPlanner.API
dotnet ef database update --project BudgetPlanner.Infrastructure --startup-project BudgetPlanner.API

## Run Client
cd budget-planner-frontend
ng serve