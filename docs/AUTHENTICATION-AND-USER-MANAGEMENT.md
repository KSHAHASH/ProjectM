# Authentication and User Management Implementation Guide

## Table of Contents

1. [Overview](#overview)
2. [Backend Implementation](#backend-implementation)
3. [Frontend Implementation](#frontend-implementation)
4. [User Interface Components](#user-interface-components)
5. [Security Considerations](#security-considerations)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

---

## Overview

This document provides a comprehensive guide to the authentication and user management system implemented in the Budget Planner application. The system uses session-based authentication with cookies, ensuring secure user login, registration, and data isolation per user account.

### Key Features

- **Session-based authentication** using ASP.NET Core Identity with HTTP-only cookies
- **Password hashing** using `PasswordHasher<User>` for secure credential storage
- **User data isolation** - each user sees only their own financial data
- **Modern UI/UX** with responsive design and gradient styling
- **Settings page** for account management and logout functionality

### Technology Stack

- **Backend**: ASP.NET Core (.NET 9.0), Entity Framework Core, SQLite
- **Frontend**: Angular, RxJS, Reactive Forms
- **Authentication**: Cookie-based sessions (7-day expiration)
- **Database**: SQLite with migrations

---

## Backend Implementation

### 1. Database Schema Changes

#### User Entity Updates

Added authentication-related fields to the `User` entity:

```csharp
// BudgetPlanner.Domain/Entities/User.cs
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // NEW: Stores hashed password
    public decimal MonthlyIncome { get; set; }
    public DateTime CreatedAt { get; set; }    // NEW: Account creation timestamp
    public DateTime? LastLogin { get; set; }   // NEW: Last login tracking
}
```

#### Database Migration

Created and applied migration to add new fields:

```bash
cd BudgetPlanner.Infrastructure
dotnet ef migrations add AddAuthenticationFields --startup-project ../BudgetPlanner.API
dotnet ef database update --startup-project ../BudgetPlanner.API
```

### 2. Authentication Controller

Created `AuthController.cs` with the following endpoints:

#### Register Endpoint

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto dto)
{
    // Check if user exists
    var existingUser = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == dto.Email);

    if (existingUser != null)
        return BadRequest(new { message = "User already exists" });

    // Create new user with hashed password
    var user = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = _passwordHasher.HashPassword(null, dto.Password),
        MonthlyIncome = dto.MonthlyIncome,
        CreatedAt = DateTime.UtcNow
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // Create authentication session
    await SignInUser(user);

    return Ok(new { message = "Registration successful", userId = user.Id });
}
```

#### Login Endpoint

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    // Find user by email
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == dto.Username);

    if (user == null)
        return Unauthorized(new { message = "Invalid credentials" });

    // Verify password
    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

    if (result == PasswordVerificationResult.Failed)
        return Unauthorized(new { message = "Invalid credentials" });

    // Update last login
    user.LastLogin = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Create authentication session
    await SignInUser(user);

    return Ok(new { message = "Login successful", userId = user.Id });
}
```

#### Logout Endpoint

```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Ok(new { message = "Logged out successfully" });
}
```

#### Get Current User Endpoint

```csharp
[Authorize]
[HttpGet("current-user")]
public async Task<IActionResult> GetCurrentUser()
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    var user = await _context.Users.FindAsync(userId);

    if (user == null)
        return NotFound();

    return Ok(new
    {
        id = user.Id,
        name = user.Name,
        email = user.Email,
        monthlyIncome = user.MonthlyIncome
    });
}
```

### 3. Authentication Configuration

Updated `Program.cs` to configure cookie authentication:

```csharp
// Add authentication services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "BudgetPlannerAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });

// Configure CORS with credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowCredentials()  // IMPORTANT: Required for cookies
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add middleware (order matters!)
app.UseCors("AllowAngular");
app.UseAuthentication();  // Must come before UseAuthorization
app.UseAuthorization();
```

### 4. Data Isolation

Updated all controllers to extract authenticated user ID from claims:

```csharp
// Before: Hardcoded user ID
var userId = 1;

// After: Extract from authenticated claims
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (string.IsNullOrEmpty(userIdClaim))
    return Unauthorized();

var userId = int.Parse(userIdClaim);
```

Applied to:

- `AnalysisController.cs` - Dashboard endpoint
- `GoalsController.cs` - Goals endpoints

### 5. Database Seeding

Updated `DbSeeder.cs` to create demo user with hashed password:

```csharp
var passwordHasher = new PasswordHasher<User>();

var demoUser = new User
{
    Name = "Demo User",
    Email = "demo@budgetplanner.com",
    PasswordHash = passwordHasher.HashPassword(null, "demo123"),
    MonthlyIncome = 5000,
    CreatedAt = DateTime.UtcNow
};
```

---

## Frontend Implementation

### 1. Authentication Service

Created `auth.service.ts` to handle all authentication operations:

```typescript
export interface User {
  id: number;
  name: string;
  email: string;
  monthlyIncome: number;
}

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private apiUrl = "http://localhost:5293/api/auth";
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadCurrentUser();
  }

  login(username: string, password: string): Observable<any> {
    return this.http
      .post(
        `${this.apiUrl}/login`,
        { username, password },
        { withCredentials: true } // IMPORTANT: Send cookies
      )
      .pipe(tap(() => this.loadCurrentUser()));
  }

  register(
    name: string,
    email: string,
    password: string,
    monthlyIncome: number
  ): Observable<any> {
    return this.http
      .post(
        `${this.apiUrl}/register`,
        { name, email, password, monthlyIncome },
        { withCredentials: true }
      )
      .pipe(tap(() => this.loadCurrentUser()));
  }

  logout(): Observable<any> {
    return this.http
      .post(`${this.apiUrl}/logout`, {}, { withCredentials: true })
      .pipe(tap(() => this.currentUserSubject.next(null)));
  }

  private loadCurrentUser(): void {
    this.http
      .get<User>(`${this.apiUrl}/current-user`, { withCredentials: true })
      .subscribe({
        next: (user) => this.currentUserSubject.next(user),
        error: () => this.currentUserSubject.next(null),
      });
  }
}
```

**Key Points:**

- `withCredentials: true` is crucial for cookie-based authentication
- `BehaviorSubject` allows components to subscribe to user state changes
- Automatically loads current user on service initialization

### 2. Auth Guard

Created `auth.guard.ts` to protect routes:

```typescript
@Injectable({
  providedIn: "root",
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): Observable<boolean> {
    return this.authService.currentUser$.pipe(
      map((user) => {
        if (user) {
          return true;
        }
        this.router.navigate(["/login"]);
        return false;
      }),
      take(1)
    );
  }
}
```

Applied to routes in `app-routing.module.ts`:

```typescript
const routes: Routes = [
  { path: "", redirectTo: "/login", pathMatch: "full" },
  { path: "login", component: LoginComponent },
  { path: "register", component: RegisterComponent },
  {
    path: "dashboard",
    component: DashboardComponent,
    canActivate: [AuthGuard], // Protected route
  },
  {
    path: "financial-input",
    component: FinancialInputComponent,
    canActivate: [AuthGuard],
  },
  {
    path: "user-profile",
    component: UserProfileComponent,
    canActivate: [AuthGuard],
  },
];
```

### 3. Auth Interceptor

Created `auth.interceptor.ts` to automatically include credentials in all HTTP requests:

```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const clonedRequest = req.clone({
      withCredentials: true,
    });
    return next.handle(clonedRequest);
  }
}
```

Registered in `app.module.ts`:

```typescript
providers: [
  {
    provide: HTTP_INTERCEPTORS,
    useClass: AuthInterceptor,
    multi: true,
  },
];
```

---

## User Interface Components

### 1. Login Component

#### Template Features

- Full-screen centered layout with purple gradient background
- Email and password validation
- Error message display
- Demo credentials display
- "Create an account" link
- Fixed positioning to prevent scrolling

#### Key Implementation Details

**TypeScript:**

```typescript
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  errorMessage = "";
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = "";

      const { username, password } = this.loginForm.value;

      this.authService.login(username, password).subscribe({
        next: (response: any) => {
          this.router.navigate(["/dashboard"]);
        },
        error: (error: any) => {
          this.errorMessage = error.error?.message || "Login failed";
          this.isLoading = false;
        },
      });
    }
  }
}
```

**Styling:**

- Purple gradient background (`#667eea` to `#764ba2`)
- White card with rounded corners and shadow
- Fixed positioning to hide sidebar
- Responsive design for mobile devices

### 2. Register Component

#### Features

- Name, email, and password fields
- Removed monthly income field (defaults to 0)
- Email format validation
- Password strength requirement (min 6 characters)
- Full-screen layout matching login page

#### Implementation

```typescript
register(): void {
  if (this.registerForm.valid) {
    const { name, email, password } = this.registerForm.value;

    this.authService.register(name, email, password, 0).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Registration failed';
      }
    });
  }
}
```

### 3. Settings Component (User Profile)

Modern, card-based settings page with two main sections:

#### Profile Information Section

```html
<div class="settings-section">
  <div class="section-card profile-card">
    <div class="card-header">
      <h3 class="section-title">Profile Information</h3>
      <p class="section-description">Your personal account details</p>
    </div>
    <div class="profile-content">
      <div class="avatar-container">
        <div class="avatar">{{ getInitials(currentUser.name) }}</div>
      </div>
      <div class="user-info">
        <div class="info-row">
          <label class="info-label">Full Name</label>
          <p class="info-value">{{ currentUser.name }}</p>
        </div>
        <div class="info-row">
          <label class="info-label">Email Address</label>
          <p class="info-value">{{ currentUser.email }}</p>
        </div>
      </div>
    </div>
  </div>
</div>
```

#### Account Actions Section

```html
<div class="settings-section">
  <div class="section-card actions-card">
    <div class="card-header">
      <h3 class="section-title">Account Actions</h3>
      <p class="section-description">
        Manage your session and account security
      </p>
    </div>
    <div class="actions-content">
      <button
        class="logout-button"
        (click)="logout()"
        [disabled]="isLoggingOut"
      >
        <span class="button-icon">🚪</span>
        <span class="button-text"
          >{{ isLoggingOut ? 'Logging out...' : 'Logout' }}</span
        >
      </button>
    </div>
  </div>
</div>
```

#### Features

- **Avatar with Initials**: Circular gradient avatar displaying user initials
- **Labeled Fields**: Clean display of user information
- **Full-Width Logout Button**: Prominent red gradient button
- **Modern Card Design**: Soft shadows, rounded corners, hover effects
- **Responsive Layout**: Mobile-friendly design

#### Helper Functions

```typescript
getInitials(name: string): string {
  if (!name) return 'U';
  const parts = name.split(' ');
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
  return name.substring(0, 2).toUpperCase();
}

logout(): void {
  if (confirm('Are you sure you want to logout?')) {
    this.isLoggingOut = true;
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Logout failed:', error);
        this.isLoggingOut = false;
        this.router.navigate(['/login']); // Redirect anyway
      }
    });
  }
}
```

### 4. Sidebar Integration

Updated `app.component.ts` to hide sidebar on authentication pages:

```typescript
export class AppComponent implements OnInit {
  showSidebar = true;

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.checkRoute(this.router.url);

    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.checkRoute(event.url);
      });
  }

  checkRoute(url: string): void {
    // Hide sidebar on auth pages
    this.showSidebar =
      !url.includes("/login") &&
      !url.includes("/register") &&
      !url.includes("/forgot-password");
  }
}
```

**Sidebar Template:**

```html
<div class="app-container" [class.no-sidebar]="!showSidebar">
  <aside class="sidebar" *ngIf="showSidebar">
    <!-- Sidebar content -->
    <a routerLink="/user-profile" routerLinkActive="active" class="nav-item">
      <i class="icon">⚙️</i>
      <span>Settings</span>
    </a>
  </aside>
  <main class="main-content">
    <router-outlet></router-outlet>
  </main>
</div>
```

**Global Styles Fix:**
Added to `styles.css` to prevent white space issues:

```css
html,
body {
  margin: 0;
  padding: 0;
  height: 100%;
  overflow-x: hidden;
}
```

---

## Security Considerations

### 1. Password Security

- **Never store plain text passwords**
- Use `PasswordHasher<User>` which implements bcrypt-style hashing
- Salting is automatic and unique per password

### 2. Cookie Security

- **HttpOnly**: Prevents JavaScript access to cookies (XSS protection)
- **SameSite=Lax**: CSRF protection while allowing normal navigation
- **Secure flag**: Should be enabled in production (HTTPS only)

### 3. CORS Configuration

- Only allow specific origins (localhost:4200 in development)
- Enable credentials explicitly
- Don't use wildcard (\*) with credentials

### 4. Route Protection

- Use `AuthGuard` on all protected routes
- Server-side validation with `[Authorize]` attribute
- Always verify user identity from claims, never trust client

### 5. Data Isolation

```csharp
// WRONG - Hardcoded or client-provided user ID
var userId = request.UserId; // Client could manipulate this!

// CORRECT - Extract from authenticated session
var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
```

---

## Testing

### Test User Credentials

```
Email: demo@budgetplanner.com
Password: demo123
```

### Manual Testing Checklist

#### Registration Flow

- [ ] Register with new email creates account
- [ ] Duplicate email shows error
- [ ] Invalid email format shows validation error
- [ ] Short password shows validation error
- [ ] Successful registration redirects to dashboard

#### Login Flow

- [ ] Valid credentials allow login
- [ ] Invalid credentials show error
- [ ] Session persists after page refresh
- [ ] Redirect to dashboard on successful login

#### Protected Routes

- [ ] Unauthenticated users redirected to login
- [ ] Authenticated users can access dashboard
- [ ] Auth guard prevents unauthorized access

#### Logout Flow

- [ ] Logout clears session
- [ ] Redirects to login page
- [ ] Cannot access protected routes after logout

#### Data Isolation

- [ ] Users see only their own expenses
- [ ] Dashboard shows correct user's data
- [ ] Multiple users don't see each other's data

### API Testing with cURL

**Register:**

```bash
curl -X POST http://localhost:5293/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "password123",
    "monthlyIncome": 5000
  }' \
  -c cookies.txt
```

**Login:**

```bash
curl -X POST http://localhost:5293/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "demo@budgetplanner.com",
    "password": "demo123"
  }' \
  -c cookies.txt
```

**Get Current User:**

```bash
curl -X GET http://localhost:5293/api/auth/current-user \
  -b cookies.txt
```

**Logout:**

```bash
curl -X POST http://localhost:5293/api/auth/logout \
  -b cookies.txt
```

---

## Troubleshooting

### Issue: "Cannot find module auth.service"

**Solution:** TypeScript cache issue. Clear Angular cache:

```bash
rm -rf .angular node_modules/.cache
ng serve
```

### Issue: Login successful but redirects to login again

**Cause:** Cookies not being sent with requests

**Solution:** Ensure `withCredentials: true` on all HTTP requests:

```typescript
// In auth.service.ts
this.http.post(url, data, { withCredentials: true });

// Or use interceptor
export class AuthInterceptor implements HttpInterceptor {
  intercept(req, next) {
    const cloned = req.clone({ withCredentials: true });
    return next.handle(cloned);
  }
}
```

### Issue: CORS errors

**Solution:** Check backend CORS configuration:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowCredentials()  // Required!
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### Issue: Sidebar showing on login page

**Solution:** Check route detection logic:

```typescript
checkRoute(url: string): void {
  this.showSidebar = !url.includes('/login') &&
                     !url.includes('/register') &&
                     !url.includes('/forgot-password');
}
```

### Issue: White space around sidebar

**Solution:** Add global CSS reset:

```css
html,
body {
  margin: 0;
  padding: 0;
  height: 100%;
}
```

### Issue: "Address already in use" when starting backend

**Solution:** Kill existing process:

```bash
# Find process
lsof -i :5293

# Kill it
kill -9 <PID>

# Or restart
pkill -f "dotnet run"
dotnet run
```

### Issue: Database changes not reflected

**Solution:** Delete and recreate database:

```bash
rm BudgetPlanner.db
dotnet ef database update
# Restart backend to trigger seeding
```

---

## File Structure Reference

```
BudgetPlanner/
├── BudgetPlanner.API/
│   ├── Controllers/
│   │   ├── AuthController.cs          # Authentication endpoints
│   │   ├── AnalysisController.cs      # Dashboard with user isolation
│   │   └── GoalsController.cs         # Goals with user isolation
│   ├── Data/
│   │   └── DbSeeder.cs                # Seed demo user with password
│   ├── Program.cs                      # Cookie auth configuration
│   └── BudgetPlanner.db               # SQLite database
│
├── BudgetPlanner.Domain/
│   └── Entities/
│       └── User.cs                     # User model with auth fields
│
├── BudgetPlanner.Infrastructure/
│   ├── Data/
│   │   └── ApplicationDbContext.cs    # EF Core context
│   └── Migrations/                     # Database migrations
│
└── budget-planner-frontend/
    └── src/app/
        ├── core/
        │   ├── services/
        │   │   └── auth.service.ts    # Authentication service
        │   ├── guards/
        │   │   └── auth.guard.ts      # Route protection
        │   └── interceptors/
        │       └── auth.interceptor.ts # Add credentials to requests
        │
        ├── features/
        │   ├── auth/
        │   │   ├── login/             # Login component
        │   │   └── register/          # Register component
        │   ├── user-profile/          # Settings component
        │   └── dashboard/             # Dashboard (protected)
        │
        ├── app.component.ts           # Sidebar visibility logic
        ├── app-routing.module.ts      # Routes with AuthGuard
        └── styles.css                 # Global CSS reset
```

---

## Best Practices Applied

1. **Separation of Concerns**: Authentication logic separated from business logic
2. **DRY Principle**: Reusable auth service and interceptor
3. **Security First**: Password hashing, HTTP-only cookies, CORS protection
4. **User Experience**: Loading states, error messages, validation feedback
5. **Responsive Design**: Mobile-friendly layouts
6. **Type Safety**: TypeScript interfaces for User and DTOs
7. **Reactive Programming**: RxJS observables for state management
8. **Route Guards**: Preventing unauthorized access
9. **Clean Code**: Consistent naming, proper comments, organized structure

---

## Future Enhancements

1. **Email Verification**: Send verification email on registration
2. **Password Reset**: Implement forgot password functionality
3. **Remember Me**: Optional extended session duration
4. **Two-Factor Authentication**: SMS or TOTP-based 2FA
5. **Social Login**: Google, Facebook, Microsoft authentication
6. **Session Management**: View and revoke active sessions
7. **Account Settings**: Change password, update profile
8. **Audit Log**: Track login history and suspicious activity
9. **Rate Limiting**: Prevent brute force attacks
10. **Refresh Tokens**: Implement token refresh mechanism

---

## Conclusion

This implementation provides a robust, secure authentication system with modern UI/UX. The session-based approach with cookies is simpler than JWT for browser-only applications and provides good security when properly configured. All user data is properly isolated, and the system is ready for production with minor adjustments (HTTPS, environment-specific configurations, etc.).

**Key Takeaways:**

- Always hash passwords, never store plain text
- Use HTTP-only cookies for session tokens
- Implement proper CORS with credentials
- Extract user ID from authenticated claims on server
- Protect routes on both client and server
- Provide clear user feedback for all actions
- Design with mobile responsiveness in mind

---

_Document created: January 18, 2026_  
_Last updated: January 18, 2026_  
_Version: 1.0_
