# 📚 How the Database Was Created - Step by Step

## 1. Configuration in `appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=BudgetPlanner.db"
}
```

- **Data Source=BudgetPlanner.db** tells Entity Framework Core to create a SQLite database file named `BudgetPlanner.db`
- This file will be created in the same directory as your API project

---

## 2. DbContext Setup in `Program.cs`

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### What this does:

- **AddDbContext<ApplicationDbContext>** - Registers the database context with dependency injection
- **UseSqlite()** - Tells EF Core to use SQLite as the database provider
- **GetConnectionString("DefaultConnection")** - Reads the connection string from `appsettings.json`

---

## 3. `ApplicationDbContext.cs` - The Database Blueprint

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<BudgetRule> BudgetRules { get; set; }
    public DbSet<Goal> Goals { get; set; }
}
```

### What this defines:

- **DbSet<User> Users** → Creates a "Users" table
- **DbSet<Expense> Expenses** → Creates an "Expenses" table
- Each DbSet becomes a table in the database

---

## 4. Creating Migrations

### Command that was run (previously):

```bash
dotnet ef migrations add InitialCreate
```

### What this did:

- Generated migration files in the **Migrations** folder
- These files contain C# code that creates SQL commands
- Example: `migrationBuilder.CreateTable("Users", ...)` → `CREATE TABLE Users (...)`

---

## 5. Applying Migrations - Creating the Physical Database

### Command I ran:

```bash
dotnet ef database update
```

### What this does line by line:

1. **Check if database exists** - If `BudgetPlanner.db` doesn't exist, create it
2. **Create migration lock table** - Creates `__EFMigrationsLock` to prevent conflicts
3. **Create migration history table** - Creates `__EFMigrationsHistory` to track which migrations have run
4. **Run migration code:**

```sql
CREATE TABLE "Users" (
  "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
  "Name" TEXT NOT NULL,
  "Email" TEXT NOT NULL,
  "MonthlyIncome" decimal(18,2) NOT NULL
)
```

5. **Create indexes:**

```sql
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
```

6. **Record migration** - Insert into `__EFMigrationsHistory` that migration was applied

---

## 6. The Result - BudgetPlanner.db File

The `BudgetPlanner.db` file contains:

- **Tables:** Users, Expenses, Goals, BudgetRules
- **Indexes:** For fast queries
- **Constraints:** Foreign keys, unique constraints
- **Metadata:** Migration history

---

## 🔄 Summary of the Flow

```
1. Domain Entities (User.cs, Expense.cs)
   ↓
2. ApplicationDbContext maps entities to tables
   ↓
3. appsettings.json specifies database file name
   ↓
4. Program.cs configures EF Core to use SQLite
   ↓
5. "dotnet ef migrations add" generates migration code
   ↓
6. "dotnet ef database update" creates BudgetPlanner.db file
   ↓
7. Physical .db file appears in your project folder
```

---

## 🗑️ The Delete Command I Ran

```bash
rm budgetPlanner.db && dotnet ef database update
```

### What this does:

- `rm budgetPlanner.db` - Deletes the physical database file
- `&&` - Then run next command
- `dotnet ef database update` - Recreates the database from scratch using migrations

---

## 📋 Key Takeaways

1. **Entity Framework Core** handles all SQL commands for you
2. **Migrations** are version control for your database schema
3. **SQLite** creates a single `.db` file that contains everything
4. **DbContext** is the bridge between your C# code and the database
5. You can recreate the database anytime using `dotnet ef database update`

---

## 🔧 Common Commands

| Command                           | Purpose                      |
| --------------------------------- | ---------------------------- |
| `dotnet ef migrations add <Name>` | Create a new migration       |
| `dotnet ef database update`       | Apply migrations to database |
| `dotnet ef migrations remove`     | Remove the last migration    |
| `dotnet ef database drop`         | Delete the database          |
| `dotnet ef migrations list`       | Show all migrations          |

---

## 📁 Database Location

The database file is created in:

```
BudgetPlanner.API/BudgetPlanner.db
```

You can open this file with SQLite browser tools to inspect the data directly.
