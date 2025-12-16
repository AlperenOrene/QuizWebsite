using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoruDeneme.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionNum = table.Column<int>(type: "int", nullable: false),
                    ChoiceA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChoiceB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChoiceC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectOption = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Question");
        }
    }
}
