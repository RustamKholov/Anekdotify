using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class SeedRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "69e734ba-1491-415d-a4d5-ff7170c93ddc", null, "User", "USER" },
                    { "9a7bb2da-9b52-4a80-b491-45aa6034aff6", null, "Admin", "ADMIN" },
                    { "d6327055-40b9-4068-8f80-a4bc6bf2b7e9", null, "Moderator", "MODERATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69e734ba-1491-415d-a4d5-ff7170c93ddc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9a7bb2da-9b52-4a80-b491-45aa6034aff6");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d6327055-40b9-4068-8f80-a4bc6bf2b7e9");
        }
    }
}
