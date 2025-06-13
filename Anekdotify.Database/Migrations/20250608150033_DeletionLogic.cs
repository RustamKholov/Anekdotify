using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class DeletionLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentRatings_Comments",
                table: "CommentRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Jokes_JokeId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_JokeLikes_Jokes",
                table: "JokeLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_JokeParts_Jokes",
                table: "JokeParts");

            migrationBuilder.DropForeignKey(
                name: "FK_JokeRatings_Jokes",
                table: "JokeRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSavedJokes_Jokes",
                table: "UserSavedJokes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSavedJokes_User",
                table: "UserSavedJokes");

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

            migrationBuilder.AlterColumn<int>(
                name: "SourceId",
                table: "Jokes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "52b00d65-b437-438d-a326-71a911b3e19e", null, "Admin", "ADMIN" },
                    { "c1b55bc9-a526-41d0-b0ca-05bb35cb1dc1", null, "User", "USER" },
                    { "e5744710-5b51-469e-aeed-dead3e59e674", null, "Moderator", "MODERATOR" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CommentRatings_Comments",
                table: "CommentRatings",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Jokes_JokeId",
                table: "Comments",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JokeLikes_Jokes",
                table: "JokeLikes",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JokeParts_Jokes",
                table: "JokeParts",
                column: "AssociatedJokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JokeRatings_Jokes",
                table: "JokeRatings",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jokes_Source",
                table: "Jokes",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "SourceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSavedJokes_Jokes",
                table: "UserSavedJokes",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSavedJokes_User",
                table: "UserSavedJokes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentRatings_Comments",
                table: "CommentRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Jokes_JokeId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_JokeLikes_Jokes",
                table: "JokeLikes");

            migrationBuilder.DropForeignKey(
                name: "FK_JokeParts_Jokes",
                table: "JokeParts");

            migrationBuilder.DropForeignKey(
                name: "FK_JokeRatings_Jokes",
                table: "JokeRatings");

            migrationBuilder.DropForeignKey(
                name: "FK_Jokes_Source",
                table: "Jokes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSavedJokes_Jokes",
                table: "UserSavedJokes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSavedJokes_User",
                table: "UserSavedJokes");

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

            migrationBuilder.AlterColumn<int>(
                name: "SourceId",
                table: "Jokes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2a8c54a7-56cb-4bbe-ae2f-ac211a9a2e53", null, "Moderator", "MODERATOR" },
                    { "3aac1cd7-f79b-4517-b243-09b455b37fc7", null, "User", "USER" },
                    { "7780245e-8f08-4896-8aac-102a492a5d4d", null, "Admin", "ADMIN" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CommentRatings_Comments",
                table: "CommentRatings",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Jokes_JokeId",
                table: "Comments",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JokeLikes_Jokes",
                table: "JokeLikes",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JokeParts_Jokes",
                table: "JokeParts",
                column: "AssociatedJokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId");

            migrationBuilder.AddForeignKey(
                name: "FK_JokeRatings_Jokes",
                table: "JokeRatings",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jokes_Source",
                table: "Jokes",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSavedJokes_Jokes",
                table: "UserSavedJokes",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "JokeId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSavedJokes_User",
                table: "UserSavedJokes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
