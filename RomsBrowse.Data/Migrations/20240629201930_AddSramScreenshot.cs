using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSramScreenshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [SRAMs]");
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "SRAMs",
                type: "varbinary(max)",
                maxLength: 1048576,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Image", "SRAMs");
        }
    }
}
