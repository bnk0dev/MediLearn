using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medilearn.Data.Migrations
{
    /// <inheritdoc />
    public partial class pdfreader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaterialText",
                table: "CourseMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialText",
                table: "CourseMaterials");
        }
    }
}
