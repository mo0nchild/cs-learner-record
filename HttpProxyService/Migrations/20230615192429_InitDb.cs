using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HttpProxyService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "methodinfo",
                schema: "public",
                columns: table => new
                {
                    MethodInfoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RequestPath = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_methodinfo", x => x.MethodInfoId);
                });

            migrationBuilder.CreateTable(
                name: "accesslogs",
                schema: "public",
                columns: table => new
                {
                    AccessLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LogData = table.Column<string>(type: "text", nullable: false),
                    MethodInfoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accesslogs", x => x.AccessLogId);
                    table.ForeignKey(
                        name: "FK_accesslogs_methodinfo_MethodInfoId",
                        column: x => x.MethodInfoId,
                        principalSchema: "public",
                        principalTable: "methodinfo",
                        principalColumn: "MethodInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_accesslogs_MethodInfoId",
                schema: "public",
                table: "accesslogs",
                column: "MethodInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accesslogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "methodinfo",
                schema: "public");
        }
    }
}
