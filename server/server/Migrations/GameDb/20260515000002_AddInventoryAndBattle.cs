using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.GameDb
{
    /// <inheritdoc />
    public partial class AddInventoryAndBattle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid        = table.Column<long>(type: "bigint", nullable: false),
                    ItemId     = table.Column<int>(type: "int", nullable: false),
                    Count      = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ObtainedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "uk_uid_item",
                table: "inventories",
                columns: new[] { "Uid", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_uid",
                table: "inventories",
                column: "Uid");

            migrationBuilder.CreateTable(
                name: "battle_records",
                columns: table => new
                {
                    Id         = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Uid        = table.Column<long>(type: "bigint", nullable: false),
                    StageId    = table.Column<int>(type: "int", nullable: false),
                    IsWin      = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RewardGold = table.Column<int>(type: "int", nullable: false),
                    CreatedAt  = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_battle_records", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_uid",
                table: "battle_records",
                column: "Uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "battle_records");
            migrationBuilder.DropTable(name: "inventories");
        }
    }
}
