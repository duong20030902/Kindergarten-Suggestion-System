using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Group03_Kindergarten_Suggestion_System_Project.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShoolOwnerId",
                table: "Schools",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_ShoolOwnerId",
                table: "Schools",
                column: "ShoolOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_AspNetUsers_ShoolOwnerId",
                table: "Schools",
                column: "ShoolOwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_AspNetUsers_ShoolOwnerId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_ShoolOwnerId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "ShoolOwnerId",
                table: "Schools");
        }
    }
}
