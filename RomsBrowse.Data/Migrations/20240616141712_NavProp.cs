using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class NavProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlatformId",
                table: "RomFiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RomFiles_PlatformId",
                table: "RomFiles",
                column: "PlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_RomFiles_Platforms_PlatformId",
                table: "RomFiles",
                column: "PlatformId",
                principalTable: "Platforms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RomFiles_Platforms_PlatformId",
                table: "RomFiles");

            migrationBuilder.DropIndex(
                name: "IX_RomFiles_PlatformId",
                table: "RomFiles");

            migrationBuilder.DropColumn(
                name: "PlatformId",
                table: "RomFiles");
        }
    }
}
