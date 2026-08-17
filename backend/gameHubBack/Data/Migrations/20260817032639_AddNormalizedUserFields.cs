using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gameHubBack.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedNickname",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // backfill existing rows from Email/Nickname before the unique indexes below are created -
            // every row would otherwise share the "" default and collide on the first unique check.
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"NormalizedEmail\" = UPPER(\"Email\"), \"NormalizedNickname\" = UPPER(\"Nickname\");");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedNickname",
                table: "Users",
                column: "NormalizedNickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedNickname",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedNickname",
                table: "Users");
        }
    }
}
