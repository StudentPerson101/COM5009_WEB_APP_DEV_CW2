using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlistTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteAppearanceSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteAppearanceSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    BannerPath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    PageBackgroundColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TextColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OutlineColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OptionButtonOutlineColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OptionButtonTextColor = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteAppearanceSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteAppearanceSettings");
        }
    }
}
