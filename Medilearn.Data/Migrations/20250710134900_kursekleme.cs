using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medilearn.Data.Migrations
{
    /// <inheritdoc />
    public partial class kursekleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialFileName",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "CourseMaterials",
                newName: "UploadDate");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "CourseMaterials",
                newName: "MaterialPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadDate",
                table: "CourseMaterials",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "MaterialPath",
                table: "CourseMaterials",
                newName: "FilePath");

            migrationBuilder.AddColumn<string>(
                name: "MaterialFileName",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
