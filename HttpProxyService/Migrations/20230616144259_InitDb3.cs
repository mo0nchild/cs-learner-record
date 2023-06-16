using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HttpProxyService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogName",
                schema: "public",
                table: "accesslogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogName",
                schema: "public",
                table: "accesslogs");
        }
    }
}
