using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationNamingAndSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SelectedDepartment",
                table: "Applications",
                newName: "University");

            migrationBuilder.RenameColumn(
                name: "SchoolName",
                table: "Applications",
                newName: "Department");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "University",
                table: "Applications",
                newName: "SelectedDepartment");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Applications",
                newName: "SchoolName");
        }
    }
}
