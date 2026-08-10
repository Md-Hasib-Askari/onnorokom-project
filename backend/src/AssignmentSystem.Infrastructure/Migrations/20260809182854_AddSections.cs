using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Grades_GradeId",
                table: "StudentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AuthUsers_TeacherId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_TeacherId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "StudentProfiles");

            migrationBuilder.RenameColumn(
                name: "GradeId",
                table: "StudentProfiles",
                newName: "SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentProfiles_GradeId",
                table: "StudentProfiles",
                newName: "IX_StudentProfiles_SectionId");

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    GradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionSubjects_AuthUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AuthUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SectionSubjects_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_GradeId_Name",
                table: "Sections",
                columns: new[] { "GradeId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SectionSubjects_SectionId_SubjectId",
                table: "SectionSubjects",
                columns: new[] { "SectionId", "SubjectId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SectionSubjects_SubjectId",
                table: "SectionSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionSubjects_TeacherId",
                table: "SectionSubjects",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Sections_SectionId",
                table: "StudentProfiles",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Sections_SectionId",
                table: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "SectionSubjects");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.RenameColumn(
                name: "SectionId",
                table: "StudentProfiles",
                newName: "GradeId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentProfiles_SectionId",
                table: "StudentProfiles",
                newName: "IX_StudentProfiles_GradeId");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Subjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "StudentProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_TeacherId",
                table: "Subjects",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Grades_GradeId",
                table: "StudentProfiles",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AuthUsers_TeacherId",
                table: "Subjects",
                column: "TeacherId",
                principalTable: "AuthUsers",
                principalColumn: "Id");
        }
    }
}
