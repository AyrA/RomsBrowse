using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddedSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveData",
                table: "SaveData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveData",
                table: "SaveData",
                columns: new[] { "UserId", "RomFileId", "Flags" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveData",
                table: "SaveData");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveData",
                table: "SaveData",
                columns: new[] { "UserId", "RomFileId" });
        }
    }
}
