using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlistTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBrowsePageHeadingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrowsePageHeadingColor",
                table: "SiteAppearanceSettings",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "#ff1744");

            migrationBuilder.Sql("UPDATE SiteAppearanceSettings SET BrowsePageHeadingColor = '#ff1744'");

            migrationBuilder.CreateTable(
                name: "BrowsePageContentSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EyebrowText = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    HeadingText = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowsePageContentSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "BrowsePageContentSettings",
                columns: new[] { "Id", "EyebrowText", "HeadingText" },
                values: new object[] { 1, "REALLY GREAT BLOCKBUSTERS", "Discover your next favourite movie" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrowsePageContentSettings");

            migrationBuilder.DropColumn(
                name: "BrowsePageHeadingColor",
                table: "SiteAppearanceSettings");
        }
    }
}
