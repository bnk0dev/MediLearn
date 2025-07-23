using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medilearn.Data.Migrations
{
    /// <inheritdoc />
    public partial class downppt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PptxFileName",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PptxFileName",
                table: "Courses");
        }
    }
}
