using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobHunter.Migrations
{
    /// <inheritdoc />
    public partial class MakeTelegramChatIdUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramChatId",
                table: "Users",
                column: "TelegramChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TelegramChatId",
                table: "Users");
        }
    }
}
