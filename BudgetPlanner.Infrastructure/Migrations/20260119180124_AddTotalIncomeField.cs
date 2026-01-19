using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalIncomeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalIncome",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalIncome",
                table: "Users");
        }
    }
}
