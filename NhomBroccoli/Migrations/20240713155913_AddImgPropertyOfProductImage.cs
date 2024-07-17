using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NhomBroccoli.Migrations
{
    /// <inheritdoc />
    public partial class AddImgPropertyOfProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Img",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Img",
                table: "ProductImages");
        }
    }
}
