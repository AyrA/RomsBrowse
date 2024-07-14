using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations.SQLite
{
    /// <inheritdoc />
    public partial class FixUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaveData_UserId_RomFileId",
                table: "SaveData");

            migrationBuilder.CreateIndex(
                name: "IX_SaveData_UserId_RomFileId_Flags",
                table: "SaveData",
                columns: new[] { "UserId", "RomFileId", "Flags" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaveData_UserId_RomFileId_Flags",
                table: "SaveData");

            migrationBuilder.CreateIndex(
                name: "IX_SaveData_UserId_RomFileId",
                table: "SaveData",
                columns: new[] { "UserId", "RomFileId" },
                unique: true);
        }
    }
}
