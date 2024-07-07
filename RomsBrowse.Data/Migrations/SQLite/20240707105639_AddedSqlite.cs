using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations.SQLite
{
    /// <inheritdoc />
    public partial class AddedSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmulatorType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Folder = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastActivity = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Flags = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RomFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Size = table.Column<int>(type: "INTEGER", nullable: false),
                    Sha256 = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: false),
                    PlatformId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RomFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RomFiles_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaveData",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RomFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Flags = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Image = table.Column<byte[]>(type: "BLOB", maxLength: 1048576, nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", maxLength: 1048576, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveData", x => new { x.UserId, x.RomFileId, x.Flags });
                    table.ForeignKey(
                        name: "FK_SaveData_RomFiles_RomFileId",
                        column: x => x.RomFileId,
                        principalTable: "RomFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaveData_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_Folder",
                table: "Platforms",
                column: "Folder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platforms_ShortName",
                table: "Platforms",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RomFiles_FileName",
                table: "RomFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_RomFiles_PlatformId",
                table: "RomFiles",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveData_Created",
                table: "SaveData",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_SaveData_RomFileId",
                table: "SaveData",
                column: "RomFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SaveData_UserId_RomFileId",
                table: "SaveData",
                columns: new[] { "UserId", "RomFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastActivity",
                table: "Users",
                column: "LastActivity");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("SaveData");
            migrationBuilder.DropTable("Settings");
            migrationBuilder.DropTable("RomFiles");
            migrationBuilder.DropTable("Users");
            migrationBuilder.DropTable("Platforms");
        }
    }
}
