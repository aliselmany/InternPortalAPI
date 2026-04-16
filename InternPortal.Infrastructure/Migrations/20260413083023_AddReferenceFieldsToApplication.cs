using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceFieldsToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceClosenessStatus",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceGsm",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ReferenceClosenessStatus",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ReferenceGsm",
                table: "Applications");
        }
    }
}
