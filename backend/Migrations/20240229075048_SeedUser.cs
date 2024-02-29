using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSpotify.Migrations
{
    /// <inheritdoc />
    public partial class SeedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_groups_users_leaderId",
                table: "groups");

            migrationBuilder.RenameColumn(
                name: "leaderId",
                table: "groups",
                newName: "leader_id");

            migrationBuilder.RenameIndex(
                name: "IX_groups_leaderId",
                table: "groups",
                newName: "IX_groups_leader_id");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "group_name", "password", "spotify_access_token", "spotify_access_token_expiration", "spotify_refresh_token", "username" },
                values: new object[] { 1L, null, "Dk8ZVuZjmsgVtJDfLv74gA3Rc4+D63N4lGH6JvauMvA=", null, null, null, "TestUser" });

            migrationBuilder.AddForeignKey(
                name: "FK_groups_users_leader_id",
                table: "groups",
                column: "leader_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_groups_users_leader_id",
                table: "groups");

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.RenameColumn(
                name: "leader_id",
                table: "groups",
                newName: "leaderId");

            migrationBuilder.RenameIndex(
                name: "IX_groups_leader_id",
                table: "groups",
                newName: "IX_groups_leaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_groups_users_leaderId",
                table: "groups",
                column: "leaderId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
