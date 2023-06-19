using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HttpProxyService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "methodinfo",
                columns: new[] { "MethodInfoId", "Name", "RequestPath" },
                values: new object[] { 7, "EmployeeMarksList", "employee_info/markslist" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 7);
        }
    }
}
