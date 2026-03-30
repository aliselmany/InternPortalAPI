using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSocialAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSocialAccount_Users_UserId",
                table: "UserSocialAccount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSocialAccount",
                table: "UserSocialAccount");

            migrationBuilder.RenameTable(
                name: "UserSocialAccount",
                newName: "UserSocialAccounts");

            migrationBuilder.RenameIndex(
                name: "IX_UserSocialAccount_UserId",
                table: "UserSocialAccounts",
                newName: "IX_UserSocialAccounts_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSocialAccounts",
                table: "UserSocialAccounts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSocialAccounts_Users_UserId",
                table: "UserSocialAccounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSocialAccounts_Users_UserId",
                table: "UserSocialAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSocialAccounts",
                table: "UserSocialAccounts");

            migrationBuilder.RenameTable(
                name: "UserSocialAccounts",
                newName: "UserSocialAccount");

            migrationBuilder.RenameIndex(
                name: "IX_UserSocialAccounts_UserId",
                table: "UserSocialAccount",
                newName: "IX_UserSocialAccount_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSocialAccount",
                table: "UserSocialAccount",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSocialAccount_Users_UserId",
                table: "UserSocialAccount",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
