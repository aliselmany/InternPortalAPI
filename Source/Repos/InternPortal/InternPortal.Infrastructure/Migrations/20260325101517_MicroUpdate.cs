using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MicroUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InternshipStartDate",
                table: "Users",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "InternshipEndDate",
                table: "Users",
                newName: "EndDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Users",
                newName: "InternshipStartDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Users",
                newName: "InternshipEndDate");
        }
    }
}
