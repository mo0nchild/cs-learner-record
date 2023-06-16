using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HttpProxyService.Migrations
{
    /// <inheritdoc />
    public partial class InitDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "public",
                table: "methodinfo",
                columns: new[] { "MethodInfoId", "Name", "RequestPath" },
                values: new object[,]
                {
                    { 1, "StudentProfile", "student_info/profile" },
                    { 2, "StudentOrders", "student_info/orders" },
                    { 3, "StudentStatements", "student_info/statements" },
                    { 4, "EmployeeProfile", "employee_info/profile" },
                    { 5, "EmployeeAttestation", "employee_info/attestation" },
                    { 6, "EmployeeSetMark", "employee_info/setmark" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "public",
                table: "methodinfo",
                keyColumn: "MethodInfoId",
                keyValue: 6);
        }
    }
}
