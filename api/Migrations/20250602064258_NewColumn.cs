using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class NewColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Jokes_JokeID",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "JokeID",
                table: "Comments",
                newName: "JokeId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_JokeID",
                table: "Comments",
                newName: "IX_Comments_JokeId");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Jokes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Jokes_JokeId",
                table: "Comments",
                column: "JokeId",
                principalTable: "Jokes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Jokes_JokeId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Jokes");

            migrationBuilder.RenameColumn(
                name: "JokeId",
                table: "Comments",
                newName: "JokeID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_JokeId",
                table: "Comments",
                newName: "IX_Comments_JokeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Jokes_JokeID",
                table: "Comments",
                column: "JokeID",
                principalTable: "Jokes",
                principalColumn: "Id");
        }
    }
}
