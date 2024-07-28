using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniServer.Migrations
{
    /// <inheritdoc />
    public partial class FixedAuthTokenStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_CreatorUserUserId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_GroupRoles_GroupRoleId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CreatorUserUserId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CreatorUserUserId",
                table: "Groups");

            migrationBuilder.AlterColumn<long>(
                name: "CreatorUserId",
                table: "Groups",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "GroupRoles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatorUserId",
                table: "Groups",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_CreatorUserId",
                table: "Groups",
                column: "CreatorUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_GroupRoles_GroupRoleId",
                table: "Permissions",
                column: "GroupRoleId",
                principalTable: "GroupRoles",
                principalColumn: "GroupRoleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Users_CreatorUserId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_GroupRoles_GroupRoleId",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CreatorUserId",
                table: "Groups");

            migrationBuilder.AlterColumn<int>(
                name: "CreatorUserId",
                table: "Groups",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserUserId",
                table: "Groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GroupRoles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatorUserUserId",
                table: "Groups",
                column: "CreatorUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Users_CreatorUserUserId",
                table: "Groups",
                column: "CreatorUserUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_GroupRoles_GroupRoleId",
                table: "Permissions",
                column: "GroupRoleId",
                principalTable: "GroupRoles",
                principalColumn: "GroupRoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
