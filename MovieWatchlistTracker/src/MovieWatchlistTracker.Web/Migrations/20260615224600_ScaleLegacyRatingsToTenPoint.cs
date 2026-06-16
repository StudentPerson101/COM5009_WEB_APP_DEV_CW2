using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MovieWatchlistTracker.Web.Data;

#nullable disable

namespace MovieWatchlistTracker.Web.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260615224600_ScaleLegacyRatingsToTenPoint")]
    public partial class ScaleLegacyRatingsToTenPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Ratings SET Score = ROUND(Score * 2.0, 1) WHERE Score BETWEEN 1 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Ratings SET Score = ROUND(Score / 2.0) WHERE Score BETWEEN 1 AND 10");
        }
    }
}
