using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSched.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDateBasedAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HolidayBlocks_StudentId",
                table: "HolidayBlocks");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilityRules_StudentId",
                table: "AvailabilityRules");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "AvailabilityRules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableDate",
                table: "AvailabilityRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AvailabilityRules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_HolidayBlocks_StudentId_StartDate_EndDate",
                table: "HolidayBlocks",
                columns: new[] { "StudentId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityRules_StudentId_AvailableDate_StartTime_EndTime",
                table: "AvailabilityRules",
                columns: new[] { "StudentId", "AvailableDate", "StartTime", "EndTime" },
                unique: true,
                filter: "[AvailableDate] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HolidayBlocks_StudentId_StartDate_EndDate",
                table: "HolidayBlocks");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilityRules_StudentId_AvailableDate_StartTime_EndTime",
                table: "AvailabilityRules");

            migrationBuilder.DropColumn(
                name: "AvailableDate",
                table: "AvailabilityRules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AvailabilityRules");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "AvailabilityRules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HolidayBlocks_StudentId",
                table: "HolidayBlocks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityRules_StudentId",
                table: "AvailabilityRules",
                column: "StudentId");
        }
    }
}
