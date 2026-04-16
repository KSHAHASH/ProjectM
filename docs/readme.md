Run server:
cd BudgetPlanner.API
dotnet build
dotnet run

Command for migration:
cd .. // go to the root
dotnet ef migrations add AddToken --project BudgetPlanner.Infrastructure --startup-project BudgetPlanner.API
dotnet ef database update --project BudgetPlanner.Infrastructure --startup-project BudgetPlanner.API

Run Client:
cd budget-planner-frontend
ng serve