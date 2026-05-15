using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.GameDb
{
    /// <inheritdoc />
    public partial class AddGameStatsAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "games",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<long>(
                name: "Gold",
                table: "games",
                type: "bigint",
                nullable: false,
                defaultValue: 1000L);

            migrationBuilder.AddColumn<int>(
                name: "Gems",
                table: "games",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.CreateTable(
                name: "gacha_history",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid        = table.Column<long>(type: "bigint", nullable: false),
                    PoolIndex  = table.Column<int>(type: "int", nullable: false),
                    ItemId     = table.Column<int>(type: "int", nullable: false),
                    Rarity     = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    GoldReward = table.Column<int>(type: "int", nullable: false),
                    CreatedAt  = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gacha_history", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_uid",
                table: "gacha_history",
                column: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "gacha_history");
            migrationBuilder.DropColumn(name: "Gems",  table: "games");
            migrationBuilder.DropColumn(name: "Gold",  table: "games");
            migrationBuilder.DropColumn(name: "Level", table: "games");
        }
    }
}
