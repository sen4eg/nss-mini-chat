using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniServer.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenEntitish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ValidationTokens_Users_Username1",
                table: "ValidationTokens");

            migrationBuilder.DropIndex(
                name: "IX_ValidationTokens_Username1",
                table: "ValidationTokens");

            migrationBuilder.DropColumn(
                name: "Username1",
                table: "ValidationTokens");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTokens_Username",
                table: "ValidationTokens",
                column: "Username");

            migrationBuilder.AddForeignKey(
                name: "FK_ValidationTokens_Users_Username",
                table: "ValidationTokens",
                column: "Username",
                principalTable: "Users",
                principalColumn: "Username",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ValidationTokens_Users_Username",
                table: "ValidationTokens");

            migrationBuilder.DropIndex(
                name: "IX_ValidationTokens_Username",
                table: "ValidationTokens");

            migrationBuilder.AddColumn<string>(
                name: "Username1",
                table: "ValidationTokens",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValidationTokens_Username1",
                table: "ValidationTokens",
                column: "Username1");

            migrationBuilder.AddForeignKey(
                name: "FK_ValidationTokens_Users_Username1",
                table: "ValidationTokens",
                column: "Username1",
                principalTable: "Users",
                principalColumn: "Username");
        }
    }
}
