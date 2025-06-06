using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class newInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Classifications",
                columns: table => new
                {
                    ClassificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classifications", x => x.ClassificationId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Role = table.Column<string>(type: "nchar(20)", fixedLength: true, maxLength: 20, nullable: false),
                    LastJokeRetrievalDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Jokes",
                columns: table => new
                {
                    JokeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubbmissionDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    SubbmitedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ClassificationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jokes", x => x.JokeId);
                    table.ForeignKey(
                        name: "FK_Jokes_Classification",
                        column: x => x.ClassificationId,
                        principalTable: "Classifications",
                        principalColumn: "ClassificationId");
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    JokeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ParentCommentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Jokes_JokeId",
                        column: x => x.JokeId,
                        principalTable: "Jokes",
                        principalColumn: "JokeId");
                    table.ForeignKey(
                        name: "FK_Comments_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "JokeLikes",
                columns: table => new
                {
                    LikeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JokeId = table.Column<int>(type: "int", nullable: false),
                    LikeDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JokeLikes", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_JokeLikes_Jokes",
                        column: x => x.JokeId,
                        principalTable: "Jokes",
                        principalColumn: "JokeId");
                    table.ForeignKey(
                        name: "FK_JokeLikes_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "JokeParts",
                columns: table => new
                {
                    JokePartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssociatedJokeId = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JokeParts", x => x.JokePartId);
                    table.ForeignKey(
                        name: "FK_JokeParts_Jokes",
                        column: x => x.AssociatedJokeId,
                        principalTable: "Jokes",
                        principalColumn: "JokeId");
                });

            migrationBuilder.CreateTable(
                name: "JokeRatings",
                columns: table => new
                {
                    RatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JokeId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<bool>(type: "bit", nullable: false),
                    RatingDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JokeRatings", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK_JokeRatings_Jokes",
                        column: x => x.JokeId,
                        principalTable: "Jokes",
                        principalColumn: "JokeId");
                    table.ForeignKey(
                        name: "FK_JokeRatings_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserSavedJokes",
                columns: table => new
                {
                    UserSavedJokeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JokeId = table.Column<int>(type: "int", nullable: false),
                    SavedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSavedJokes", x => x.UserSavedJokeId);
                    table.ForeignKey(
                        name: "FK_UserSavedJokes_Jokes",
                        column: x => x.JokeId,
                        principalTable: "Jokes",
                        principalColumn: "JokeId");
                    table.ForeignKey(
                        name: "FK_UserSavedJokes_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CommentRatings",
                columns: table => new
                {
                    CommentRateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<bool>(type: "bit", nullable: false),
                    RatingDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentRatings", x => x.CommentRateId);
                    table.ForeignKey(
                        name: "FK_CommentRatings_Comments",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "CommentId");
                    table.ForeignKey(
                        name: "FK_CommentRatings_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentRatings",
                table: "CommentRatings",
                columns: new[] { "CommentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentRatings_UserId",
                table: "CommentRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_JokeId",
                table: "Comments",
                column: "JokeId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JokeLikes_UserId",
                table: "JokeLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_UserJokeLike",
                table: "JokeLikes",
                columns: new[] { "JokeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JokeParts_AssociatedJokeId",
                table: "JokeParts",
                column: "AssociatedJokeId");

            migrationBuilder.CreateIndex(
                name: "IX_JokeRatings_JokeId",
                table: "JokeRatings",
                column: "JokeId");

            migrationBuilder.CreateIndex(
                name: "IX_JokeRatings_UserId",
                table: "JokeRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jokes_ClassificationId",
                table: "Jokes",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedJokes",
                table: "UserSavedJokes",
                columns: new[] { "JokeId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSavedJokes_UserId",
                table: "UserSavedJokes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentRatings");

            migrationBuilder.DropTable(
                name: "JokeLikes");

            migrationBuilder.DropTable(
                name: "JokeParts");

            migrationBuilder.DropTable(
                name: "JokeRatings");

            migrationBuilder.DropTable(
                name: "UserSavedJokes");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Jokes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Classifications");
        }
    }
}
