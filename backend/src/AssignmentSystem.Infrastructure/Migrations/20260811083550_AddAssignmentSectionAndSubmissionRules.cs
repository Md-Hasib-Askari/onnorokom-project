using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentSectionAndSubmissionRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions");

            migrationBuilder.AddColumn<Guid>(
                name: "SectionId",
                table: "Assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId_StudentId",
                table: "Submissions",
                columns: new[] { "AssignmentId", "StudentId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SectionId",
                table: "Assignments",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Sections_SectionId",
                table: "Assignments",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Sections_SectionId",
                table: "Assignments");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_AssignmentId_StudentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_SectionId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Assignments");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions",
                column: "AssignmentId");
        }
    }
}
