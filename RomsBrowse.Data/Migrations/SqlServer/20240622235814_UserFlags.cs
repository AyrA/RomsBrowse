using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RomsBrowse.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Flags",
                table: "Users",
                type: "int",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("Flags", "Users");
        }
    }
}
