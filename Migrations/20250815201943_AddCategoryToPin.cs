using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PinterestClone.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Pins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Pins");
        }
    }
}
