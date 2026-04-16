using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TranscriptUrl",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranscriptUrl",
                table: "Applications");
        }
    }
}
