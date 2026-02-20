using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.GameDb
{
    /// <inheritdoc />
    public partial class cc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "games",
                newName: "CreatedTimeUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedTimeUtc",
                table: "games",
                newName: "CreatedAt");
        }
    }
}
