using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartialUniqueIndexesIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Code_GradeId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Grades_Name_AcademicYear",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_AuthUsers_Email",
                table: "AuthUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code_GradeId",
                table: "Subjects",
                columns: new[] { "Code", "GradeId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_Name_AcademicYear",
                table: "Grades",
                columns: new[] { "Name", "AcademicYear" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AuthUsers_Email",
                table: "AuthUsers",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Code_GradeId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Grades_Name_AcademicYear",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_AuthUsers_Email",
                table: "AuthUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code_GradeId",
                table: "Subjects",
                columns: new[] { "Code", "GradeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_Name_AcademicYear",
                table: "Grades",
                columns: new[] { "Name", "AcademicYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthUsers_Email",
                table: "AuthUsers",
                column: "Email",
                unique: true);
        }
    }
}
