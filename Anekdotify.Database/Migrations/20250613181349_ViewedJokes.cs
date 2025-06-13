using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class ViewedJokes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "52b00d65-b437-438d-a326-71a911b3e19e");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c1b55bc9-a526-41d0-b0ca-05bb35cb1dc1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e5744710-5b51-469e-aeed-dead3e59e674");

            migrationBuilder.CreateTable(
                name: "UserViewedJokes",
                columns: table => new
                {
                    UserViewedJokeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JokeId = table.Column<int>(type: "int", nullable: false),
                    ViewedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserViewedJokes", x => x.UserViewedJokeId);
                    table.ForeignKey(
                        name: "FK_UserViewedJokes_Jokes",
                        column: x => x.JokeId,
                        principalTable: "Jokes",
                        principalColumn: "JokeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserViewedJokes_User",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2db25d37-5d9b-41a3-9580-e7d9d8b560ea", null, "Admin", "ADMIN" },
                    { "99055f38-7760-46b7-8211-2ed0ed671418", null, "Moderator", "MODERATOR" },
                    { "c5438f73-efdb-4401-b9cf-27d79ac8ab5c", null, "User", "USER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserViewedJokes",
                table: "UserViewedJokes",
                columns: new[] { "JokeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserViewedJokes_UserId",
                table: "UserViewedJokes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserViewedJokes");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2db25d37-5d9b-41a3-9580-e7d9d8b560ea");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "99055f38-7760-46b7-8211-2ed0ed671418");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c5438f73-efdb-4401-b9cf-27d79ac8ab5c");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "52b00d65-b437-438d-a326-71a911b3e19e", null, "Admin", "ADMIN" },
                    { "c1b55bc9-a526-41d0-b0ca-05bb35cb1dc1", null, "User", "USER" },
                    { "e5744710-5b51-469e-aeed-dead3e59e674", null, "Moderator", "MODERATOR" }
                });
        }
    }
}
