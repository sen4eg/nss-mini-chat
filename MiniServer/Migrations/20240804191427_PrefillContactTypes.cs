using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniServer.Migrations
{
    /// <inheritdoc />
    public partial class PrefillContactTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ContactTypes",
                columns: new[] { "ContactTypeId", "Type" },
                values: new object[,]
                {
                    { 0, "Stranger" },
                    { 1, "Friend" },
                    { 2, "BlockedBy" },
                    { 3, "BlockedHim"},
                    { 4, "Group" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ContactTypes",
                keyColumn: "ContactTypeId",
                keyValues: new object[] { 0, 1, 2, 3, 4 });
        }
    }
}