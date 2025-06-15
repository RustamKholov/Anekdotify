using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class SourceFetchedJokes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4f3b364a-f8fc-4cf4-90d0-cf948f19fbfc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "961e604e-e1ad-4c15-9945-ed41176f9ec5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9f5abc02-c993-49f8-b6fa-239fcfb1752a");

            migrationBuilder.CreateTable(
                name: "SourceFetchedJokes",
                columns: table => new
                {
                    SourceFetchedJokeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FetchedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    SourceJokeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceFetchedJokes", x => x.SourceFetchedJokeId);
                    table.ForeignKey(
                        name: "FK_SourceFetchedJokes_Source",
                        column: x => x.SourceId,
                        principalTable: "Sources",
                        principalColumn: "SourceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "11f4bec5-4723-4407-aa66-3c9a3f1d4dcb", null, "Moderator", "MODERATOR" },
                    { "1b6a9731-1201-48e0-bc19-74e0781ec4d5", null, "User", "USER" },
                    { "da738c5e-138e-4f84-96a7-5d5538a76125", null, "Admin", "ADMIN" }
                });

            migrationBuilder.InsertData(
                table: "Sources",
                columns: new[] { "SourceId", "SourceName" },
                values: new object[] { 1, "JokeAPI" });

            migrationBuilder.CreateIndex(
                name: "IX_SourceFetchedJokes_SourceId",
                table: "SourceFetchedJokes",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SourceFetchedJokes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "11f4bec5-4723-4407-aa66-3c9a3f1d4dcb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1b6a9731-1201-48e0-bc19-74e0781ec4d5");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "da738c5e-138e-4f84-96a7-5d5538a76125");

            migrationBuilder.DeleteData(
                table: "Sources",
                keyColumn: "SourceId",
                keyValue: 1);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4f3b364a-f8fc-4cf4-90d0-cf948f19fbfc", null, "Moderator", "MODERATOR" },
                    { "961e604e-e1ad-4c15-9945-ed41176f9ec5", null, "User", "USER" },
                    { "9f5abc02-c993-49f8-b6fa-239fcfb1752a", null, "Admin", "ADMIN" }
                });
        }
    }
}
