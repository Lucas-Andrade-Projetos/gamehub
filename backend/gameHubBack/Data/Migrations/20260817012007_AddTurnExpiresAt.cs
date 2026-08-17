using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gameHubBack.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TurnExpiresAt",
                table: "BatalhaRuralGames",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurnExpiresAt",
                table: "BatalhaRuralGames");
        }
    }
}
