using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedKanbanTemplateBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KanbanBoardTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanBoardTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanbanBoardTemplates_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KanbanTemplateTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KanbanTemplateTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KanbanTemplateTasks_KanbanBoardTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "KanbanBoardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KanbanBoardTemplates_StaffId",
                table: "KanbanBoardTemplates",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_KanbanTemplateTasks_TemplateId",
                table: "KanbanTemplateTasks",
                column: "TemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KanbanTemplateTasks");

            migrationBuilder.DropTable(
                name: "KanbanBoardTemplates");
        }
    }
}
