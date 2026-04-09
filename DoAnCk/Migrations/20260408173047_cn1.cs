using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCk.Migrations
{
    /// <inheritdoc />
    public partial class cn1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "District",
                table: "Rooms");
        }
    }
}
