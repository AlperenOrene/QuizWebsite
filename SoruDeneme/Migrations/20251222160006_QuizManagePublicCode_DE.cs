using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoruDeneme.Migrations
{
    /// <inheritdoc />
    public partial class QuizManagePublicCode_DE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuizName",
                table: "Quiz",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessCode",
                table: "Quiz",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Quiz",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OwnerTeacherId",
                table: "Quiz",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChoiceD",
                table: "Question",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChoiceE",
                table: "Question",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_OwnerTeacherId",
                table: "Quiz",
                column: "OwnerTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Users_OwnerTeacherId",
                table: "Quiz",
                column: "OwnerTeacherId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Users_OwnerTeacherId",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_OwnerTeacherId",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "AccessCode",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "OwnerTeacherId",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "ChoiceD",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "ChoiceE",
                table: "Question");

            migrationBuilder.AlterColumn<string>(
                name: "QuizName",
                table: "Quiz",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Question",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
