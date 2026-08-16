using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gameHubBack.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "BatalhaRuralPlayer",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BatalhaRuralPlayer");
        }
    }
}
