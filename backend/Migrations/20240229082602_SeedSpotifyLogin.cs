using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YSpotify.Migrations
{
    /// <inheritdoc />
    public partial class SeedSpotifyLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "spotify_access_token", "spotify_access_token_expiration", "spotify_refresh_token" },
                values: new object[] { "BQAb7XgAXhct_76b67aYGEd87CrbmwWht_nrM2by_sWBbbuMwgPU0AMbd_GEnHhEa4dpPwAraQ74FrVVUi0EJs7JCk8ZKkkkuBPzQXO87NlL1JuuCEanSwCYxPWxoWvUE_JTAq_2C7qdF_ZgIos1j24C1q9oxk6_Y95qpFs_-V6Dj697vPQRnIhnXOpxwZw1guzi5zNOXybeBYTpKkKfRZaSjJiiwjzN4WlAq1G_uIG2EU0e7kPKhjMPoPDM1PR2MYAzC_Ll1Q6UNWU_oGWl7w", 1709195411L, "AQC40pbscT1uJqiPWb-PVw8xy9QkDt0bEz0K8VGmUCKXWf7ySZgQw_fWom4e8-npHXYA_UKjBr2A39mmQXtAt4YRUFQ1rGFXrFhAlBqIpuYKhyeAc5NEQcXjMOfk-jkJORw" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "spotify_access_token", "spotify_access_token_expiration", "spotify_refresh_token" },
                values: new object[] { null, null, null });
        }
    }
}
