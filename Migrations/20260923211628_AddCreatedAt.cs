using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobHunter.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Vacancies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vacancies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeenAt",
                table: "Vacancies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_Source_ExternalId",
                table: "Vacancies",
                columns: new[] { "Source", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vacancies_Source_ExternalId",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "LastSeenAt",
                table: "Vacancies");
        }
    }
}
