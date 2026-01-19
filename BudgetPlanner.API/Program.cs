using BudgetPlanner.Application.Interfaces;
using BudgetPlanner.Application.Services;
using BudgetPlanner.Infrastructure.Data;
using  BudgetPlanner.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure CORS to allow credentials (cookies)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Enable cookies/credentials
        });
});

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "BudgetPlannerAuth";
        options.Cookie.HttpOnly = true; // Prevent JavaScript access
        options.Cookie.SameSite = SameSiteMode.Lax; // Allow cookies in same-site requests
        options.Cookie.SecurePolicy = CookieSecurePolicy.None; // For development (use Always in production)
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401; // Return 401 instead of redirect
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON to accept enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework Core with SQLite
// This tells EF Core to use a SQLite database with the connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IScenarioService, ScenarioService>();
builder.Services.AddScoped<IMonthlyAnalysisService, MonthlyAnalysisService>();
builder.Services.AddScoped<IRecommendationsService, RecommendationsService>();

var app = builder.Build();

// Seed the database with initial demo user
DbSeeder.SeedDatabase(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
