using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HttpProxyService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTime",
                schema: "public",
                table: "accesslogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessTime",
                schema: "public",
                table: "accesslogs");
        }
    }
}
