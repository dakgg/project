using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.GameDb
{
    /// <inheritdoc />
    public partial class AddUidAndGachaStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Uid",
                table: "games",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "idx_uid",
                table: "games",
                column: "Uid");

            migrationBuilder.CreateTable(
                name: "gacha_states",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid = table.Column<long>(type: "bigint", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    PityCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UpdatedTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gacha_states", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "uk_uid_index",
                table: "gacha_states",
                columns: new[] { "Uid", "Index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_uid",
                table: "gacha_states",
                column: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "gacha_states");
            migrationBuilder.DropIndex(name: "idx_uid", table: "games");
            migrationBuilder.DropColumn(name: "Uid", table: "games");
        }
    }
}
