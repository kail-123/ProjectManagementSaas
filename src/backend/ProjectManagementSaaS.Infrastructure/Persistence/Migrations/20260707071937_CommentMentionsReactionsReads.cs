using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommentMentionsReactionsReads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentMentions_UserProfiles_UserProfileId",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentMentions",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                schema: "work",
                table: "CommentMentions",
                newName: "MentionedUserId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentMentions_UserProfileId",
                schema: "work",
                table: "CommentMentions",
                newName: "IX_CommentMentions_MentionedUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "work",
                table: "CommentMentions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "work",
                table: "CommentMentions",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<Guid>(
                name: "MentionedByUserId",
                schema: "work",
                table: "CommentMentions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE mentions
                SET MentionedUserId = profiles.UserId
                FROM work.CommentMentions AS mentions
                INNER JOIN organization.UserProfiles AS profiles
                    ON mentions.MentionedUserId = profiles.Id;
                """);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentMentions",
                schema: "work",
                table: "CommentMentions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CommentReactions",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReactions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "work",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReactions_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentReads",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReads_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "work",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentReads_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_CommentId_MentionedUserId",
                schema: "work",
                table: "CommentMentions",
                columns: new[] { "CommentId", "MentionedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_MentionedByUserId",
                schema: "work",
                table: "CommentMentions",
                column: "MentionedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_CommentId_Emoji",
                schema: "work",
                table: "CommentReactions",
                columns: new[] { "CommentId", "Emoji" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_CommentId_UserId_Emoji",
                schema: "work",
                table: "CommentReactions",
                columns: new[] { "CommentId", "UserId", "Emoji" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_UserId",
                schema: "work",
                table: "CommentReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentReads_CommentId_UserId",
                schema: "work",
                table: "CommentReads",
                columns: new[] { "CommentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentReads_UserId_ReadAt",
                schema: "work",
                table: "CommentReads",
                columns: new[] { "UserId", "ReadAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommentMentions_UserProfiles_MentionedByUserId",
                schema: "work",
                table: "CommentMentions",
                column: "MentionedByUserId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CommentMentions_UserProfiles_MentionedUserId",
                schema: "work",
                table: "CommentMentions",
                column: "MentionedUserId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentMentions_UserProfiles_MentionedByUserId",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentMentions_UserProfiles_MentionedUserId",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropTable(
                name: "CommentReactions",
                schema: "work");

            migrationBuilder.DropTable(
                name: "CommentReads",
                schema: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommentMentions",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropIndex(
                name: "IX_CommentMentions_CommentId_MentionedUserId",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropIndex(
                name: "IX_CommentMentions_MentionedByUserId",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.DropColumn(
                name: "MentionedByUserId",
                schema: "work",
                table: "CommentMentions");

            migrationBuilder.Sql(
                """
                UPDATE mentions
                SET MentionedUserId = profiles.Id
                FROM work.CommentMentions AS mentions
                INNER JOIN organization.UserProfiles AS profiles
                    ON mentions.MentionedUserId = profiles.UserId;
                """);

            migrationBuilder.RenameColumn(
                name: "MentionedUserId",
                schema: "work",
                table: "CommentMentions",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_CommentMentions_MentionedUserId",
                schema: "work",
                table: "CommentMentions",
                newName: "IX_CommentMentions_UserProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommentMentions",
                schema: "work",
                table: "CommentMentions",
                columns: new[] { "CommentId", "UserProfileId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommentMentions_UserProfiles_UserProfileId",
                schema: "work",
                table: "CommentMentions",
                column: "UserProfileId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
