using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInternTransferTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InternTransferRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternTransferRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternTransferRequests_Users_FromStaffId",
                        column: x => x.FromStaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternTransferRequests_Users_InternId",
                        column: x => x.InternId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InternTransferRequests_Users_ToStaffId",
                        column: x => x.ToStaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InternTransferRequests_FromStaffId",
                table: "InternTransferRequests",
                column: "FromStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_InternTransferRequests_InternId",
                table: "InternTransferRequests",
                column: "InternId");

            migrationBuilder.CreateIndex(
                name: "IX_InternTransferRequests_ToStaffId",
                table: "InternTransferRequests",
                column: "ToStaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternTransferRequests");
        }
    }
}
