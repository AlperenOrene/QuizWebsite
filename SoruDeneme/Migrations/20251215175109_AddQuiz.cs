using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoruDeneme.Migrations
{
    /// <inheritdoc />
    public partial class AddQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "Question",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Quiz",
                columns: table => new
                {
                    QuizId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quiz", x => x.QuizId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuizId",
                table: "Question",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_Quiz_QuizId",
                table: "Question",
                column: "QuizId",
                principalTable: "Quiz",
                principalColumn: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_Quiz_QuizId",
                table: "Question");

            migrationBuilder.DropTable(
                name: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Question_QuizId",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "Question");
        }
    }
}
