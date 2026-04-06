using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAnCk.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoomRequestAndFixNulls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZaloNumber",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "RoomRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "RoomRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoomId1",
                table: "RoomRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomRequests_RoomId1",
                table: "RoomRequests",
                column: "RoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomRequests_Rooms_RoomId1",
                table: "RoomRequests",
                column: "RoomId1",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomRequests_Rooms_RoomId1",
                table: "RoomRequests");

            migrationBuilder.DropIndex(
                name: "IX_RoomRequests_RoomId1",
                table: "RoomRequests");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ZaloNumber",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "RoomRequests");

            migrationBuilder.DropColumn(
                name: "RoomId1",
                table: "RoomRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "RoomRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
