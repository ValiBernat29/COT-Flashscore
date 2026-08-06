using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlashscoreBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSimpleLineups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwayLineup",
                table: "Fixtures",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "HomeLineup",
                table: "Fixtures",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayLineup",
                table: "Fixtures");

            migrationBuilder.DropColumn(
                name: "HomeLineup",
                table: "Fixtures");
        }
    }
}
