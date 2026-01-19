using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableBalanceToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AvailableBalance",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableBalance",
                table: "Users");
        }
    }
}
