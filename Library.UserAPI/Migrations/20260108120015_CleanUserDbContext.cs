using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Library.UserAPI.Migrations
{
    /// <inheritdoc />
    public partial class CleanUserDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { -1, -1 });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "ArchivedByUserId", "ArchivedDate", "ConcurrencyStamp", "CreatedByUserId", "CreatedDate", "LastModifiedByUserId", "LastModifiedDate", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { -2, null, null, "34a763db-cfc7-4f76-89b5-44567d71e35b", null, new DateOnly(2026, 1, 8), null, null, "Normal", "NORMAL" },
                    { -1, null, null, "bce68817-03b4-47f6-9a18-b51e2191e9c0", null, new DateOnly(2026, 1, 8), null, null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "ArchivedByUserId", "ArchivedDate", "BorrowCount", "ConcurrencyStamp", "CreatedByUserId", "CreatedDate", "DeactivatedByUserId", "DeactivatedDate", "Email", "EmailConfirmed", "LastModifiedByUserId", "LastModifiedDate", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { -1, 0, null, null, 0, "18852a65-763a-441c-a406-b669da564b9b", null, new DateOnly(2026, 1, 8), null, null, "admin@library.local", true, null, null, false, null, "ADMIN@LIBRARY.LOCAL", "ADMIN", "AQAAAAIAAYagAAAAELMy+daEaqmhJowJ3ybrquUNjkz6k4+xy3EweNKQQeJ/ik3mezTMPtNbgjsttY6dJQ==", null, false, "bfac52a8-af70-4b51-b43b-c4e4bb67815a", false, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { -1, -1 });
        }
    }
}
