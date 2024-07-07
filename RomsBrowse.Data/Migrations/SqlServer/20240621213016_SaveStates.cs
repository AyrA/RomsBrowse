using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class SaveStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaveStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RomFileId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaveStates_RomFiles_RomFileId",
                        column: x => x.RomFileId,
                        principalTable: "RomFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaveStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaveStates_Created",
                table: "SaveStates",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_SaveStates_RomFileId",
                table: "SaveStates",
                column: "RomFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveStates_UserId_RomFileId",
                table: "SaveStates",
                columns: ["UserId", "RomFileId"],
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaveStates");
        }
    }
}
