using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class DitchIdAddSRAM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveStates",
                table: "SaveStates");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SaveStates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveStates",
                table: "SaveStates",
                columns: ["UserId", "RomFileId"]);

            migrationBuilder.CreateTable(
                name: "SRAMs",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RomFileId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SRAMs", x => new { x.UserId, x.RomFileId });
                    table.ForeignKey(
                        name: "FK_SRAMs_RomFiles_RomFileId",
                        column: x => x.RomFileId,
                        principalTable: "RomFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SRAMs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SRAMs_Created",
                table: "SRAMs",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_SRAMs_RomFileId",
                table: "SRAMs",
                column: "RomFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SRAMs_UserId_RomFileId",
                table: "SRAMs",
                columns: ["UserId", "RomFileId"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SRAMs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaveStates",
                table: "SaveStates");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "SaveStates",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaveStates",
                table: "SaveStates",
                column: "Id");
        }
    }
}
