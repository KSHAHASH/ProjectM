# 📘 LINQ and Entity Framework Core - Complete Guide

A comprehensive reference for LINQ queries and Entity Framework Core operations used to interact with database tables.

---

## Table of Contents

1. [What is LINQ?](#what-is-linq)
2. [What is Entity Framework Core?](#what-is-entity-framework-core)
3. [Reading Data (Query Operations)](#reading-data-query-operations)
4. [Writing Data (Modification Operations)](#writing-data-modification-operations)
5. [Aggregation Operations](#aggregation-operations)
6. [Async vs Sync Operations](#async-vs-sync-operations)
7. [Common Patterns in Your Code](#common-patterns-in-your-code)
8. [Quick Reference Table](#quick-reference-table)

---

## What is LINQ?

**LINQ** = **L**anguage **IN**tegrated **Q**uery

- A C# feature that lets you query collections (lists, arrays, databases) using C# syntax
- Converts your C# code into SQL queries when working with Entity Framework
- Makes database operations type-safe and readable

**Example:**

```csharp
// Instead of SQL: SELECT * FROM Users WHERE Id = 1
var user = _dbContext.Users.Where(u => u.Id == 1);
```

---

## What is Entity Framework Core?

**Entity Framework Core** (EF Core) is an **ORM** (Object-Relational Mapper):

- Translates C# objects to database tables
- Translates LINQ queries to SQL
- Handles database connections automatically
- Tracks changes to objects and saves them to database

**Your DbContext:**

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }        // → Users table
    public DbSet<Expense> Expenses { get; set; }  // → Expenses table
    public DbSet<Goal> Goals { get; set; }        // → Goals table
}
```

---

## Reading Data (Query Operations)

### 1. **Where** - Filter Records

**Purpose:** Filter records based on a condition (like SQL `WHERE` clause)

**Syntax:**

```csharp
.Where(item => condition)
```

**Examples from your code:**

```csharp
// Get all expenses for a specific user
var expenses = await _dbContext.Expenses
    .Where(e => e.UserId == userId)
    .ToListAsync();

// Multiple conditions with AND (&&)
var expensesInJanuary = expenses
    .Where(e => e.Date.Month == 1 && e.Date.Year == 2026);

// Multiple conditions with OR (||)
var highExpenses = expenses
    .Where(e => e.Amount > 1000 || e.Category == ExpenseCategory.Emergency);
```

**SQL Generated:**

```sql
SELECT * FROM Expenses WHERE UserId = @userId
```

---

### 2. **Select** - Transform Data

**Purpose:** Project/transform data into a different shape (like SQL `SELECT` clause)

**Syntax:**

```csharp
.Select(item => new { /* properties */ })
```

**Examples:**

```csharp
// Select only specific fields
var expenseAmounts = expenses.Select(e => e.Amount);
// Result: [100, 200, 500, 300]

// Create anonymous objects
var expenseSummary = expenses.Select(e => new
{
    Category = e.Category,
    Amount = e.Amount
});

// From your code: Extract year and month
var months = expenses.Select(e => new { e.Date.Year, e.Date.Month });
```

**SQL Generated:**

```sql
SELECT Amount FROM Expenses
```

---

### 3. **OrderBy / OrderByDescending** - Sort Data

**Purpose:** Sort data in ascending or descending order (like SQL `ORDER BY`)

**Syntax:**

```csharp
.OrderBy(item => field)          // Ascending (A→Z, 1→9)
.OrderByDescending(item => field) // Descending (Z→A, 9→1)
.ThenBy(item => field)           // Secondary sort (ascending)
.ThenByDescending(item => field) // Secondary sort (descending)
```

**Examples:**

```csharp
// Sort by date (oldest first)
var expenses = await _dbContext.Expenses
    .OrderBy(e => e.Date)
    .ToListAsync();

// Sort by amount (highest first)
var expensesByAmount = expenses
    .OrderByDescending(e => e.Amount);

// Multiple sorts: Year, then Month
var monthlyExpenses = expenses
    .OrderBy(x => x.Year)
    .ThenBy(x => x.Month);

// From your code: Find most expensive category
var topCategory = categoryBreakdown
    .OrderByDescending(kvp => kvp.Value)
    .First();
```

**SQL Generated:**

```sql
SELECT * FROM Expenses ORDER BY Date ASC
SELECT * FROM Expenses ORDER BY Amount DESC
```

---

### 4. **FirstOrDefault / FirstOrDefaultAsync** - Get First Item

**Purpose:** Get the first item that matches a condition, or `null` if none found

**Syntax:**

```csharp
.FirstOrDefault()              // First item or null
.FirstOrDefault(item => condition) // First match or null
```

**Examples:**

```csharp
// Get user by ID
var user = await _dbContext.Users
    .FirstOrDefaultAsync(u => u.Id == userId);

if (user == null)
{
    throw new InvalidOperationException("User not found");
}

// Get first expense (no condition)
var firstExpense = expenses.FirstOrDefault();
```

**Similar Methods:**

- **First()** - Throws exception if not found (use when you're sure it exists)
- **Single()** - Ensures exactly one match, throws if 0 or 2+ matches
- **Last()** - Get last item

**SQL Generated:**

```sql
SELECT TOP(1) * FROM Users WHERE Id = @userId
```

---

### 5. **Find / FindAsync** - Get by Primary Key

**Purpose:** Quick way to get a record by its primary key (ID)

**Syntax:**

```csharp
.Find(id)
.FindAsync(id)
```

**Examples:**

```csharp
// Get user by ID (primary key)
var user = await _dbContext.Users.FindAsync(userId);

// Find is faster than FirstOrDefault for primary keys
// EF Core checks the in-memory cache first
```

**SQL Generated:**

```sql
SELECT * FROM Users WHERE Id = @userId
```

---

### 6. **GroupBy** - Group Records

**Purpose:** Group records by a common field (like SQL `GROUP BY`)

**Syntax:**

```csharp
.GroupBy(item => groupingField)
```

**Examples:**

```csharp
// Group expenses by category
var categoryBreakdown = expenses
    .GroupBy(e => e.Category)
    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
// Result: { Food: 500, Transport: 300, Shopping: 700 }

// From your code: Group by Year and Month
var monthlyExpenses = expenses
    .GroupBy(e => new { e.Date.Year, e.Date.Month })
    .Select(g => new
    {
        Year = g.Key.Year,
        Month = g.Key.Month,
        Amount = g.Sum(e => e.Amount)
    });

// Group by expense type and count
var typeDistribution = expenses
    .GroupBy(e => e.Type)
    .ToDictionary(g => g.Key, g => g.Count());
// Result: { Fixed: 10, Variable: 25, OneTime: 5 }
```

**Parts of GroupBy:**

- **g.Key** - The value you grouped by (Category, Month, etc.)
- **g** - The group of items (all expenses in that category)
- **g.Sum()** - Aggregate function on the group

**SQL Generated:**

```sql
SELECT Category, SUM(Amount) FROM Expenses GROUP BY Category
```

---

### 7. **ToList / ToListAsync** - Execute Query

**Purpose:** Execute the query and get results as a List

**Syntax:**

```csharp
.ToList()       // Synchronous (blocking)
.ToListAsync()  // Asynchronous (non-blocking)
```

**Examples:**

```csharp
// Execute query and get all results
var expenses = await _dbContext.Expenses
    .Where(e => e.UserId == userId)
    .ToListAsync();

// Without ToList, query is not executed (lazy evaluation)
var query = _dbContext.Expenses.Where(e => e.UserId == userId);
// ↑ No database call yet

var results = await query.ToListAsync();
// ↑ NOW the database is queried
```

**Important:**

- LINQ queries are **lazy** - they don't execute until you call `ToList()`, `First()`, `Count()`, etc.
- Use `ToListAsync()` in async methods for better performance

---

### 8. **Distinct** - Remove Duplicates

**Purpose:** Get unique values only (like SQL `DISTINCT`)

**Syntax:**

```csharp
.Distinct()
```

**Examples:**

```csharp
// From your code: Get unique months with expenses
var monthsWithExpenses = expenses
    .Select(e => new { e.Date.Year, e.Date.Month })
    .Distinct()
    .Count();
// Result: 5 (if expenses exist in 5 different months)

// Get unique categories used
var categoriesUsed = expenses
    .Select(e => e.Category)
    .Distinct();
```

**SQL Generated:**

```sql
SELECT DISTINCT Year, Month FROM Expenses
```

---

### 9. **Any** - Check if Records Exist

**Purpose:** Returns `true` if any records match, `false` otherwise

**Syntax:**

```csharp
.Any()              // Are there any items?
.Any(item => condition) // Do any items match?
```

**Examples:**

```csharp
// From your code: Check if there are any expenses
if (!expenseList.Any())
{
    return new SpendingBehaviorDto { /* empty */ };
}

// Check if user has any high expenses
bool hasHighExpenses = expenses.Any(e => e.Amount > 5000);

// Check if category exists
bool hasFoodExpenses = expenses.Any(e => e.Category == ExpenseCategory.Food);
```

**SQL Generated:**

```sql
SELECT CASE WHEN EXISTS(SELECT 1 FROM Expenses) THEN 1 ELSE 0 END
```

---

### 10. **ToDictionary** - Convert to Dictionary

**Purpose:** Convert a collection to a Dictionary (key-value pairs)

**Syntax:**

```csharp
.ToDictionary(item => keySelector, item => valueSelector)
```

**Examples:**

```csharp
// From your code: Category → Total Amount
var categoryBreakdown = expenseList
    .GroupBy(e => e.Category)
    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
// Result: Dictionary<ExpenseCategory, decimal>
// { Food: 1500, Transport: 800, Shopping: 1200 }

// Type → Count
var typeDistribution = expenseList
    .GroupBy(e => e.Type)
    .ToDictionary(g => g.Key, g => g.Count());
// Result: Dictionary<ExpenseType, int>
// { Fixed: 10, Variable: 25 }
```

**Access:**

```csharp
decimal foodTotal = categoryBreakdown[ExpenseCategory.Food]; // 1500
int fixedCount = typeDistribution[ExpenseType.Fixed]; // 10
```

---

## Writing Data (Modification Operations)

### 1. **Add / AddAsync** - Insert New Record

**Purpose:** Add a new record to the database (SQL `INSERT`)

**Syntax:**

```csharp
_dbContext.TableName.Add(entity)
_dbContext.TableName.AddAsync(entity)
```

**Examples:**

```csharp
// Add a single expense
var expense = new Expense
{
    UserId = userId,
    Category = ExpenseCategory.Food,
    Amount = 50.00m,
    Date = DateTime.UtcNow
};
_dbContext.Expenses.Add(expense);
await _dbContext.SaveChangesAsync();

// From your code: Add multiple expenses
var expenses = expenseList.Select(e => new Expense
{
    UserId = userId,
    Category = e.Category,
    Amount = e.Amount,
    Date = e.Date ?? DateTime.UtcNow,
    Type = e.Type
}).ToList();

_dbContext.Expenses.AddRange(expenses);
await _dbContext.SaveChangesAsync();
```

**SQL Generated:**

```sql
INSERT INTO Expenses (UserId, Category, Amount, Date, Type)
VALUES (@userId, @category, @amount, @date, @type)
```

**Key Points:**

- `Add()` marks the entity for insertion
- Nothing happens until `SaveChangesAsync()` is called
- Use `AddRange()` for multiple items (more efficient)

---

### 2. **Update** - Modify Existing Record

**Purpose:** Update an existing record (SQL `UPDATE`)

**Syntax:**

```csharp
_dbContext.TableName.Update(entity)
```

**Examples:**

```csharp
// From your code: Update user's income
var user = await _dbContext.Users.FindAsync(userId);
if (user != null)
{
    user.MonthlyIncome += income; // Modify property
    _dbContext.Users.Update(user);
    await _dbContext.SaveChangesAsync();
}

// Update expense amount
var expense = await _dbContext.Expenses.FindAsync(expenseId);
expense.Amount = 100.00m;
_dbContext.Expenses.Update(expense);
await _dbContext.SaveChangesAsync();
```

**Alternative (Change Tracking):**

```csharp
// EF Core automatically tracks changes
var user = await _dbContext.Users.FindAsync(userId);
user.MonthlyIncome += income; // Just modify
await _dbContext.SaveChangesAsync(); // No Update() needed!
```

**SQL Generated:**

```sql
UPDATE Users SET MonthlyIncome = @income WHERE Id = @userId
```

**Key Points:**

- If you get entity from database, EF Core tracks changes automatically
- `Update()` is needed when entity is not tracked (e.g., created manually)

---

### 3. **Remove** - Delete Record

**Purpose:** Delete a record (SQL `DELETE`)

**Syntax:**

```csharp
_dbContext.TableName.Remove(entity)
```

**Examples:**

```csharp
// Delete a specific expense
var expense = await _dbContext.Expenses.FindAsync(expenseId);
if (expense != null)
{
    _dbContext.Expenses.Remove(expense);
    await _dbContext.SaveChangesAsync();
}

// Delete multiple expenses
var oldExpenses = await _dbContext.Expenses
    .Where(e => e.Date.Year < 2020)
    .ToListAsync();

_dbContext.Expenses.RemoveRange(oldExpenses);
await _dbContext.SaveChangesAsync();
```

**SQL Generated:**

```sql
DELETE FROM Expenses WHERE Id = @expenseId
```

---

### 4. **SaveChanges / SaveChangesAsync** - Commit to Database

**Purpose:** Execute all pending changes (INSERT, UPDATE, DELETE) in the database

**Syntax:**

```csharp
_dbContext.SaveChanges()       // Synchronous
_dbContext.SaveChangesAsync()  // Asynchronous
```

**Examples:**

```csharp
// From your code: Save all changes
_dbContext.Expenses.AddRange(expenses);
await _dbContext.SaveChangesAsync(); // Commits to database

// Multiple operations in one transaction
var user = await _dbContext.Users.FindAsync(userId);
user.MonthlyIncome += income;

var expense = new Expense { /* ... */ };
_dbContext.Expenses.Add(expense);

// Both changes saved together (transaction)
await _dbContext.SaveChangesAsync();
```

**Key Points:**

- All changes are tracked in memory until `SaveChangesAsync()` is called
- If `SaveChangesAsync()` fails, **all** changes are rolled back (transaction)
- Returns the number of records affected

---

## Aggregation Operations

### 1. **Sum** - Calculate Total

**Purpose:** Add up all values

```csharp
// Total expenses
var totalExpenses = expenses.Sum(e => e.Amount);

// Sum by group
var categoryTotals = expenses
    .GroupBy(e => e.Category)
    .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
```

**SQL:** `SELECT SUM(Amount) FROM Expenses`

---

### 2. **Count** - Count Records

**Purpose:** Count how many records match

```csharp
// Total number of expenses
var expenseCount = expenses.Count();

// Count with condition
var highExpenseCount = expenses.Count(e => e.Amount > 1000);

// From your code: Count unique months
var monthsWithExpenses = expenses
    .Select(e => new { e.Date.Year, e.Date.Month })
    .Distinct()
    .Count();
```

**SQL:** `SELECT COUNT(*) FROM Expenses`

---

### 3. **Average** - Calculate Average

**Purpose:** Calculate mean value

```csharp
// From your code: Average expense amount
var averageExpenseAmount = expenseList.Average(e => e.Amount);

// Average by category
var avgFoodExpense = expenses
    .Where(e => e.Category == ExpenseCategory.Food)
    .Average(e => e.Amount);
```

**SQL:** `SELECT AVG(Amount) FROM Expenses`

---

### 4. **Max / Min** - Find Maximum/Minimum

**Purpose:** Find highest or lowest value

```csharp
// Highest expense
var maxExpense = expenses.Max(e => e.Amount);

// Lowest expense
var minExpense = expenses.Min(e => e.Amount);

// Most recent expense date
var latestDate = expenses.Max(e => e.Date);
```

**SQL:** `SELECT MAX(Amount) FROM Expenses`

---

## Async vs Sync Operations

### When to Use Async

**Always use async in:**

- Web API controllers
- Services that access database
- Any I/O operation (database, file, network)

### Common Async Methods

| Synchronous        | Asynchronous            | Use Case           |
| ------------------ | ----------------------- | ------------------ |
| `ToList()`         | `ToListAsync()`         | Get all results    |
| `FirstOrDefault()` | `FirstOrDefaultAsync()` | Get first match    |
| `Find()`           | `FindAsync()`           | Get by primary key |
| `Count()`          | `CountAsync()`          | Count records      |
| `Any()`            | `AnyAsync()`            | Check if exists    |
| `SaveChanges()`    | `SaveChangesAsync()`    | Commit changes     |

### Example Pattern

```csharp
public async Task<DashboardDto> GetDashboardDataAsync(int userId)
//     ^^^^^       ^^^^                             ^^^^^
//   async      Returns Task                    Async suffix
{
    // Use await with async methods
    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    //         ^^^^^                   ^^^^^^^^^^^^^^^^^^

    var expenses = await _dbContext.Expenses.ToListAsync();
    //             ^^^^^                     ^^^^^^^^^^^

    await _dbContext.SaveChangesAsync();
    //    ^^^^^                  ^^^^^

    return dashboardDto; // No await needed (not async operation)
}
```

---

## Common Patterns in Your Code

### Pattern 1: Query → Filter → Sort → Execute

```csharp
var expenses = await _dbContext.Expenses    // Start with table
    .Where(e => e.UserId == userId)         // Filter
    .OrderBy(e => e.Date)                   // Sort
    .ToListAsync();                         // Execute
```

---

### Pattern 2: Get Data → Group → Aggregate → Transform

```csharp
// From your AnalysisService
var monthlyExpenses = expenses              // Input data
    .GroupBy(e => new { e.Date.Year, e.Date.Month })  // Group by month
    .Select(g => new                        // Transform each group
    {
        Year = g.Key.Year,
        Month = g.Key.Month,
        Amount = g.Sum(e => e.Amount)       // Aggregate
    })
    .OrderBy(x => x.Year)                   // Sort
    .ThenBy(x => x.Month)
    .ToList();                              // Execute
```

---

### Pattern 3: Check Existence → Modify → Save

```csharp
// Get entity
var user = await _dbContext.Users.FindAsync(userId);

// Check if exists
if (user != null)
{
    // Modify
    user.MonthlyIncome += income;
    _dbContext.Users.Update(user);

    // Save
    await _dbContext.SaveChangesAsync();
}
```

---

### Pattern 4: Convert DTO → Entity → Save

```csharp
// From your CalculateAndSaveFinancialHealthAsync
var expenses = expenseList.Select(e => new Expense  // DTO → Entity
{
    UserId = userId,
    Category = e.Category,
    Amount = e.Amount,
    Date = e.Date ?? DateTime.UtcNow,
    Type = e.Type
}).ToList();

_dbContext.Expenses.AddRange(expenses);  // Add to context
await _dbContext.SaveChangesAsync();     // Save to database
```

---

## Quick Reference Table

### Reading Operations

| Method                  | Purpose           | Returns       | Example                                |
| ----------------------- | ----------------- | ------------- | -------------------------------------- |
| `Where()`               | Filter records    | IQueryable    | `.Where(e => e.Amount > 100)`          |
| `Select()`              | Transform data    | IQueryable    | `.Select(e => e.Amount)`               |
| `OrderBy()`             | Sort ascending    | IQueryable    | `.OrderBy(e => e.Date)`                |
| `OrderByDescending()`   | Sort descending   | IQueryable    | `.OrderByDescending(e => e.Amount)`    |
| `GroupBy()`             | Group records     | IQueryable    | `.GroupBy(e => e.Category)`            |
| `FirstOrDefaultAsync()` | First or null     | Task<T>       | `.FirstOrDefaultAsync(e => e.Id == 1)` |
| `FindAsync()`           | Get by ID         | Task<T>       | `.FindAsync(userId)`                   |
| `ToListAsync()`         | Execute query     | Task<List<T>> | `.ToListAsync()`                       |
| `Any()`                 | Check existence   | bool          | `.Any(e => e.Amount > 100)`            |
| `Distinct()`            | Remove duplicates | IQueryable    | `.Distinct()`                          |

### Aggregation Operations

| Method      | Purpose         | Returns        | Example                   |
| ----------- | --------------- | -------------- | ------------------------- |
| `Sum()`     | Total of values | decimal/int    | `.Sum(e => e.Amount)`     |
| `Count()`   | Number of items | int            | `.Count()`                |
| `Average()` | Mean value      | decimal/double | `.Average(e => e.Amount)` |
| `Max()`     | Highest value   | T              | `.Max(e => e.Amount)`     |
| `Min()`     | Lowest value    | T              | `.Min(e => e.Amount)`     |

### Writing Operations

| Method               | Purpose     | SQL Equivalent    | Example                                 |
| -------------------- | ----------- | ----------------- | --------------------------------------- |
| `Add()`              | Insert one  | INSERT            | `_dbContext.Expenses.Add(expense)`      |
| `AddRange()`         | Insert many | INSERT (multiple) | `_dbContext.Expenses.AddRange(list)`    |
| `Update()`           | Modify      | UPDATE            | `_dbContext.Users.Update(user)`         |
| `Remove()`           | Delete one  | DELETE            | `_dbContext.Expenses.Remove(expense)`   |
| `RemoveRange()`      | Delete many | DELETE (multiple) | `_dbContext.Expenses.RemoveRange(list)` |
| `SaveChangesAsync()` | Commit      | COMMIT            | `await _dbContext.SaveChangesAsync()`   |

---

## Best Practices

### ✅ DO:

- Use `async`/`await` for database operations
- Use `ToListAsync()` to execute queries
- Check for `null` after `FirstOrDefaultAsync()` or `FindAsync()`
- Use `AddRange()` for multiple inserts (more efficient)
- Use meaningful variable names

### ❌ DON'T:

- Don't use `ToList()` in async methods (use `ToListAsync()`)
- Don't forget `await` keyword
- Don't forget `SaveChangesAsync()` after modifications
- Don't query inside loops (causes N+1 problem)

### Example of N+1 Problem (DON'T DO THIS):

```csharp
// BAD: Queries database for each user (100 users = 101 queries!)
foreach (var expense in expenses)
{
    var user = await _dbContext.Users.FindAsync(expense.UserId); // ❌
}

// GOOD: Query once and use in-memory join
var userIds = expenses.Select(e => e.UserId).Distinct();
var users = await _dbContext.Users
    .Where(u => userIds.Contains(u.Id))
    .ToListAsync(); // ✅ Single query
```

---

## Summary

**LINQ** provides a powerful, type-safe way to query data in C#:

- **Where** = Filter
- **Select** = Transform
- **OrderBy** = Sort
- **GroupBy** = Group
- **Sum/Count/Average** = Aggregate

**Entity Framework Core** handles database operations:

- **Add** = Insert
- **Update** = Modify
- **Remove** = Delete
- **SaveChanges** = Commit

**Always use async methods** in web applications for better performance and scalability!

---

This guide covers all the main operations you'll use 90% of the time. Refer back to this when you need a quick reminder! 🚀
