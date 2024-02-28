using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YSpotify.Migrations
{
    /// <inheritdoc />
    public partial class SeedDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    name = table.Column<string>(type: "text", nullable: false),
                    leaderId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    group_name = table.Column<string>(type: "text", nullable: true),
                    spotify_access_token_expiration = table.Column<long>(type: "bigint", nullable: true),
                    spotify_access_token = table.Column<string>(type: "text", nullable: true),
                    spotify_refresh_token = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_groups_group_name",
                        column: x => x.group_name,
                        principalTable: "groups",
                        principalColumn: "name");
                });

            migrationBuilder.CreateIndex(
                name: "IX_groups_leaderId",
                table: "groups",
                column: "leaderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_group_name",
                table: "users",
                column: "group_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_groups_users_leaderId",
                table: "groups",
                column: "leaderId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_groups_users_leaderId",
                table: "groups");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "groups");
        }
    }
}
