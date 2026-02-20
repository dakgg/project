using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.UserDb
{
    /// <inheritdoc />
    public partial class aa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "LastLoginTimeUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTimeUtc",
                table: "users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTimeUtc",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "LastLoginTimeUtc",
                table: "users",
                newName: "CreatedAt");
        }
    }
}
