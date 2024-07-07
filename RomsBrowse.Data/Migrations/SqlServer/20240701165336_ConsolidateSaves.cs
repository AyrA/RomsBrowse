using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateSaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaveData",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RomFileId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Flags = table.Column<int>(type: "int", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false)
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
                columns: ["UserId", "RomFileId", "Flags"],
                unique: true);

            //Migrate data over to new table
            migrationBuilder.Sql(@"
INSERT INTO [SaveData] (UserId,RomFileId,Created,Flags,Image,Data)
SELECT UserId,RomFileId,Created,1 as Flags,Image,Data FROM [SaveStates]");

            migrationBuilder.Sql(@"
INSERT INTO [SaveData] (UserId,RomFileId,Created,Flags,Image,Data)
SELECT UserId,RomFileId,Created,2 as Flags,Image,Data FROM [SRAMs]");

            migrationBuilder.DropTable("SaveStates");
            migrationBuilder.DropTable("SRAMs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SaveStates",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RomFileId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveStates", x => new { x.UserId, x.RomFileId });
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

            migrationBuilder.CreateTable(
                name: "SRAMs",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RomFileId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", maxLength: 1048576, nullable: false)
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


            //Migrate data over to old table
            migrationBuilder.Sql(@"
INSERT INTO [SaveStates] (UserId,RomFileId,Created,Image,Data)
SELECT UserId,RomFileId,Created,Image,Data FROM [SaveData] WHERE (Flags&1)=1");

            migrationBuilder.Sql(@"
INSERT INTO [SRAMs] (UserId,RomFileId,Created,Image,Data)
SELECT UserId,RomFileId,Created,Image,Data FROM [SaveData] WHERE (Flags&2)=2");

            migrationBuilder.DropTable("SaveData");
        }
    }
}
