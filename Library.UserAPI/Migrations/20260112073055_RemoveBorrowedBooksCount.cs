using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBorrowedBooksCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorrowCount",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BorrowCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
