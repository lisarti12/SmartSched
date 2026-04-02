using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSched.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaxStudyHoursFromAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStudyHours",
                table: "AvailabilityRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxStudyHours",
                table: "AvailabilityRules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
