using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Group03_Kindergarten_Suggestion_System_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableSchoolEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Desctiption",
                table: "SchoolEnrollments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Desctiption",
                table: "SchoolEnrollments");
        }
    }
}
