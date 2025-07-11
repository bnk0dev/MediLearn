using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medilearn.Data.Migrations
{
    /// <inheritdoc />
    public partial class ikinciMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserTCNo",
                table: "Courses",
                type: "nvarchar(11)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserTCNo",
                table: "Courses",
                column: "UserTCNo");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_UserTCNo",
                table: "Courses",
                column: "UserTCNo",
                principalTable: "Users",
                principalColumn: "TCNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_UserTCNo",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UserTCNo",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "UserTCNo",
                table: "Courses");
        }
    }
}
