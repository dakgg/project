using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.UserDb
{
    /// <inheritdoc />
    public partial class AddPublicKeyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Day 5: 로그인 쿼리 (WHERE PublicKey = ?) 성능을 위한 인덱스
            // 로그인은 가장 빈번한 쿼리이므로 인덱스 필수
            migrationBuilder.CreateIndex(
                name: "idx_public_key",
                table: "users",
                column: "PublicKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_public_key",
                table: "users");
        }
    }
}
