using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCk.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTable : Migration
    {
        /// <inheritdoc />
       
            protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ví dụ: Thêm một cột mới vào bảng Users
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
