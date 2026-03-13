using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSched.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorClassCanvasFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Semester = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProfessorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseClasses_AspNetUsers_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CourseContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseClassId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedHours = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseContentItems_CourseClasses_CourseClassId",
                        column: x => x.CourseClassId,
                        principalTable: "CourseClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseClassId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_CourseClasses_CourseClassId",
                        column: x => x.CourseClassId,
                        principalTable: "CourseClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseClasses_ProfessorId",
                table: "CourseClasses",
                column: "ProfessorId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseContentItems_CourseClassId",
                table: "CourseContentItems",
                column: "CourseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseClassId",
                table: "CourseEnrollments",
                column: "CourseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentId",
                table: "CourseEnrollments",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseContentItems");

            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "CourseClasses");
        }
    }
}
