using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastLogin",
                table: "Users",
                newName: "LastActivity");

            migrationBuilder.RenameIndex(
                name: "IX_Users_LastLogin",
                table: "Users",
                newName: "IX_Users_LastActivity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastActivity",
                table: "Users",
                newName: "LastLogin");

            migrationBuilder.RenameIndex(
                name: "IX_Users_LastActivity",
                table: "Users",
                newName: "IX_Users_LastLogin");
        }
    }
}
