using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeSubjectCodeOptionalAndNameUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Code_GradeId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_GradeId",
                table: "Subjects");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Subjects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_GradeId_Name",
                table: "Subjects",
                columns: new[] { "GradeId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_GradeId_Name",
                table: "Subjects");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Subjects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code_GradeId",
                table: "Subjects",
                columns: new[] { "Code", "GradeId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_GradeId",
                table: "Subjects",
                column: "GradeId");
        }
    }
}
