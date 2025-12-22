using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoruDeneme.Migrations
{
    public partial class AddOwnerTeacherToQuizNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Kolonu ekle (nullable)
            migrationBuilder.AddColumn<int>(
                name: "OwnerTeacherId",
                table: "Quiz",
                type: "int",
                nullable: true);

            // 2) FK çakışmasını kesin olarak engelle:
            //    - Users içinde en az 1 eğitmen yoksa oluştur
            //    - Quiz.OwnerTeacherId geçersizse onu bu eğitmene bağla
            migrationBuilder.Sql(@"
                DECLARE @teacherId INT;

                -- Var olan bir eğitmeni bul
                SELECT TOP 1 @teacherId = [Id]
                FROM [Users]
                WHERE [Role] = N'Egitmen'
                ORDER BY [Id];

                -- Yoksa bir tane oluştur
                IF @teacherId IS NULL
                BEGIN
                    INSERT INTO [Users] ([Username], [PasswordHash], [Role])
                    VALUES (N'eğitmen', N'dummy_hash', N'Egitmen');

                    SET @teacherId = SCOPE_IDENTITY();
                END

                -- OwnerTeacherId geçersizse düzelt (NULL / 0 / Users'ta yok)
                UPDATE [Quiz]
                SET [OwnerTeacherId] = @teacherId
                WHERE [OwnerTeacherId] IS NULL
                   OR [OwnerTeacherId] = 0
                   OR [OwnerTeacherId] NOT IN (SELECT [Id] FROM [Users]);
            ");

            // 3) Index
            migrationBuilder.CreateIndex(
                name: "IX_Quiz_OwnerTeacherId",
                table: "Quiz",
                column: "OwnerTeacherId");

            // 4) FK
            migrationBuilder.AddForeignKey(
                name: "FK_Quiz_Users_OwnerTeacherId",
                table: "Quiz",
                column: "OwnerTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quiz_Users_OwnerTeacherId",
                table: "Quiz");

            migrationBuilder.DropIndex(
                name: "IX_Quiz_OwnerTeacherId",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "OwnerTeacherId",
                table: "Quiz");
        }
    }
}
