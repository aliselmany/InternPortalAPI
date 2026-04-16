using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TranscriptUrl",
                table: "Applications",
                newName: "TranscriptFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TranscriptFile",
                table: "Applications",
                newName: "TranscriptUrl");
        }
    }
}
