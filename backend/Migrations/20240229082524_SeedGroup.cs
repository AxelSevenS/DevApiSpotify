using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSpotify.Migrations
{
    /// <inheritdoc />
    public partial class SeedGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_groups_leader_id",
                table: "groups");

            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "name", "leader_id" },
                values: new object[] { "TestGroup", 1L });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                column: "group_name",
                value: "TestGroup");

            migrationBuilder.CreateIndex(
                name: "IX_groups_leader_id",
                table: "groups",
                column: "leader_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_groups_leader_id",
                table: "groups");

            migrationBuilder.DeleteData(
                table: "groups",
                keyColumn: "name",
                keyValue: "TestGroup");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                column: "group_name",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_groups_leader_id",
                table: "groups",
                column: "leader_id",
                unique: true);
        }
    }
}
