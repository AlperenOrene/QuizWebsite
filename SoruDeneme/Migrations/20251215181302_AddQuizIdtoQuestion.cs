using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoruDeneme.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizIdtoQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuizId",
                table: "Quiz",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Quiz",
                newName: "QuizId");
        }
    }
}
