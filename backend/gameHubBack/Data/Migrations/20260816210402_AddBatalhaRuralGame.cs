using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gameHubBack.Migrations
{
    /// <inheritdoc />
    public partial class AddBatalhaRuralGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatalhaRuralGames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatalhaRuralGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatalhaRuralPlayer",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<string>(type: "text", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: false),
                    PlayerNum = table.Column<int>(type: "integer", nullable: false),
                    PlayerStatus = table.Column<int>(type: "integer", nullable: false),
                    BoardTiles = table.Column<string>(type: "jsonb", nullable: false),
                    Tokens = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatalhaRuralPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatalhaRuralPlayer_BatalhaRuralGames_GameId",
                        column: x => x.GameId,
                        principalTable: "BatalhaRuralGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatalhaRuralPlayer_GameId",
                table: "BatalhaRuralPlayer",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatalhaRuralPlayer");

            migrationBuilder.DropTable(
                name: "BatalhaRuralGames");
        }
    }
}
