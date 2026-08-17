using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gameHubBack.Migrations
{
    /// <inheritdoc />
    public partial class AddBattleState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShotsReceived",
                table: "BatalhaRuralPlayer",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<int>(
                name: "CurrentTurnPlayerNum",
                table: "BatalhaRuralGames",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShotsReceived",
                table: "BatalhaRuralPlayer");

            migrationBuilder.DropColumn(
                name: "CurrentTurnPlayerNum",
                table: "BatalhaRuralGames");
        }
    }
}
