using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class SourceTableInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Jokes");

            migrationBuilder.AlterColumn<string>(
                name: "SubbmitedByUserId",
                table: "Jokes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "Jokes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "Jokes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sources",
                columns: table => new
                {
                    SourceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.SourceId);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2a8c54a7-56cb-4bbe-ae2f-ac211a9a2e53", null, "Moderator", "MODERATOR" },
                    { "3aac1cd7-f79b-4517-b243-09b455b37fc7", null, "User", "USER" },
                    { "7780245e-8f08-4896-8aac-102a492a5d4d", null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "SourceId", "SourceName" },
                values: new object[,]
                {
                    { -3, "Generated" },
                    { -2, "System" },
                    { -1, "From User" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jokes_SourceId",
                table: "Jokes",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jokes_Source",
                table: "Jokes",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jokes_Source",
                table: "Jokes");

            migrationBuilder.DropTable(
                name: "Sources");

            migrationBuilder.DropIndex(
                name: "IX_Jokes_SourceId",
                table: "Jokes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2a8c54a7-56cb-4bbe-ae2f-ac211a9a2e53");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3aac1cd7-f79b-4517-b243-09b455b37fc7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7780245e-8f08-4896-8aac-102a492a5d4d");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Jokes");

            migrationBuilder.AlterColumn<string>(
                name: "SubbmitedByUserId",
                table: "Jokes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByUserId",
                table: "Jokes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Jokes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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
    }
}
