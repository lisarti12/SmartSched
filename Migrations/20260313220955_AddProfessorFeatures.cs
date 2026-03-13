using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSched.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "CourseContentItems");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "CourseContentItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "CourseContentItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CourseContentItems",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LectureItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseClassId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LectureItems_CourseClasses_CourseClassId",
                        column: x => x.CourseClassId,
                        principalTable: "CourseClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LectureItems_CourseClassId",
                table: "LectureItems",
                column: "CourseClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LectureItems");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CourseContentItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "CourseContentItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedHours",
                table: "CourseContentItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "CourseContentItems",
                type: "int",
                nullable: true);
        }
    }
}
