using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssignmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenGracePeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousRefreshToken",
                table: "AuthUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PreviousRefreshTokenGraceExpiresAt",
                table: "AuthUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousRefreshToken",
                table: "AuthUsers");

            migrationBuilder.DropColumn(
                name: "PreviousRefreshTokenGraceExpiresAt",
                table: "AuthUsers");
        }
    }
}
