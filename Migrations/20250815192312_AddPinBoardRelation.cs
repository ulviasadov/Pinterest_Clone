using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PinterestClone.Migrations
{
    /// <inheritdoc />
    public partial class AddPinBoardRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pins_Boards_BoardId",
                table: "Pins");

            migrationBuilder.DropIndex(
                name: "IX_Pins_BoardId",
                table: "Pins");

            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "Pins");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Pins",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Boards",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "PinBoards",
                columns: table => new
                {
                    PinId = table.Column<int>(type: "int", nullable: false),
                    BoardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinBoards", x => new { x.PinId, x.BoardId });
                    table.ForeignKey(
                        name: "FK_PinBoards_Boards_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Boards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PinBoards_Pins_PinId",
                        column: x => x.PinId,
                        principalTable: "Pins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PinBoards_BoardId",
                table: "PinBoards",
                column: "BoardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PinBoards");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Pins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoardId",
                table: "Pins",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Boards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pins_BoardId",
                table: "Pins",
                column: "BoardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pins_Boards_BoardId",
                table: "Pins",
                column: "BoardId",
                principalTable: "Boards",
                principalColumn: "Id");
        }
    }
}
