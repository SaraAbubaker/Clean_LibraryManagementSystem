using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: -1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: -1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: -1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: -1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: -1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Publishers",
                keyColumn: "Id",
                keyValue: -1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
