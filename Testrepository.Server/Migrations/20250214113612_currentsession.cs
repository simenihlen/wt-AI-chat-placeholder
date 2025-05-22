using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Testrepository.Server.Migrations
{
    /// <inheritdoc />
    public partial class currentsession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentSessionid",
                schema: "testschema",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_CurrentSessionid",
                schema: "testschema",
                table: "users",
                column: "CurrentSessionid");

            migrationBuilder.AddForeignKey(
                name: "FK_users_sessions_CurrentSessionid",
                schema: "testschema",
                table: "users",
                column: "CurrentSessionid",
                principalSchema: "testschema",
                principalTable: "sessions",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_sessions_CurrentSessionid",
                schema: "testschema",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_CurrentSessionid",
                schema: "testschema",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CurrentSessionid",
                schema: "testschema",
                table: "users");
        }
    }
}
