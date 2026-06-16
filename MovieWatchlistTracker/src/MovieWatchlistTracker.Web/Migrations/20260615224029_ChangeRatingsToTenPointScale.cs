using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlistTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRatingsToTenPointScale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ratings_Score",
                table: "Ratings");

            migrationBuilder.AlterColumn<double>(
                name: "Score",
                table: "Ratings",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ratings_Score",
                table: "Ratings",
                sql: "Score BETWEEN 1 AND 10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ratings_Score",
                table: "Ratings");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "Ratings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ratings_Score",
                table: "Ratings",
                sql: "Score BETWEEN 1 AND 5");
        }
    }
}
