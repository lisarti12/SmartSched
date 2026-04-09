using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSched.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseContentItemId",
                table: "TaskItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HolidayBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HolidayBlocks_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentTaskNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseClassId = table.Column<int>(type: "int", nullable: false),
                    CourseContentItemId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTaskNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTaskNotifications_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTaskNotifications_CourseClasses_CourseClassId",
                        column: x => x.CourseClassId,
                        principalTable: "CourseClasses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentTaskNotifications_CourseContentItems_CourseContentItemId",
                        column: x => x.CourseContentItemId,
                        principalTable: "CourseContentItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CourseContentItemId",
                table: "TaskItems",
                column: "CourseContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HolidayBlocks_StudentId",
                table: "HolidayBlocks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTaskNotifications_CourseClassId",
                table: "StudentTaskNotifications",
                column: "CourseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTaskNotifications_CourseContentItemId",
                table: "StudentTaskNotifications",
                column: "CourseContentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTaskNotifications_StudentId",
                table: "StudentTaskNotifications",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_CourseContentItems_CourseContentItemId",
                table: "TaskItems",
                column: "CourseContentItemId",
                principalTable: "CourseContentItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_CourseContentItems_CourseContentItemId",
                table: "TaskItems");

            migrationBuilder.DropTable(
                name: "HolidayBlocks");

            migrationBuilder.DropTable(
                name: "StudentTaskNotifications");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CourseContentItemId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "CourseContentItemId",
                table: "TaskItems");
        }
    }
}
