using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlistTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationBarTextColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NavigationBarTextColor",
                table: "SiteAppearanceSettings",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "#39ff14");

            migrationBuilder.Sql("UPDATE SiteAppearanceSettings SET NavigationBarTextColor = TextColor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NavigationBarTextColor",
                table: "SiteAppearanceSettings");
        }
    }
}
