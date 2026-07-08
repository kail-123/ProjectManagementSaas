using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseThreadedCommentEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommentHistory_CommentId",
                schema: "work",
                table: "CommentHistory");

            migrationBuilder.RenameColumn(
                name: "BodyMarkdown",
                schema: "work",
                table: "Comments",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "PreviousBodyMarkdown",
                schema: "work",
                table: "CommentHistory",
                newName: "PreviousMessage");

            migrationBuilder.RenameColumn(
                name: "ChangedOn",
                schema: "work",
                table: "CommentHistory",
                newName: "EditedAt");

            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                schema: "work",
                table: "CommentHistory",
                newName: "EditedByName");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                schema: "work",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EditedAt",
                schema: "work",
                table: "Comments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Pinned",
                schema: "work",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Resolved",
                schema: "work",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "EditedBy",
                schema: "work",
                table: "CommentHistory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewMessage",
                schema: "work",
                table: "CommentHistory",
                type: "nvarchar(max)",
                maxLength: 8192,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CommentAttachments",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    PreviewType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentAttachments_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "work",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                schema: "work",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WorkItemId_ParentCommentId_CreatedOn",
                schema: "work",
                table: "Comments",
                columns: new[] { "WorkItemId", "ParentCommentId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WorkItemId_Pinned_CreatedOn",
                schema: "work",
                table: "Comments",
                columns: new[] { "WorkItemId", "Pinned", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WorkItemId_Resolved_CreatedOn",
                schema: "work",
                table: "Comments",
                columns: new[] { "WorkItemId", "Resolved", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentHistory_CommentId_EditedAt",
                schema: "work",
                table: "CommentHistory",
                columns: new[] { "CommentId", "EditedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentHistory_EditedBy",
                schema: "work",
                table: "CommentHistory",
                column: "EditedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CommentAttachments_CommentId_FileName",
                schema: "work",
                table: "CommentAttachments",
                columns: new[] { "CommentId", "FileName" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentHistory_UserProfiles_EditedBy",
                schema: "work",
                table: "CommentHistory",
                column: "EditedBy",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_UserProfiles_AuthorId",
                schema: "work",
                table: "Comments",
                column: "AuthorId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentHistory_UserProfiles_EditedBy",
                schema: "work",
                table: "CommentHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_UserProfiles_AuthorId",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "CommentAttachments",
                schema: "work");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AuthorId",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_WorkItemId_ParentCommentId_CreatedOn",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_WorkItemId_Pinned_CreatedOn",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_WorkItemId_Resolved_CreatedOn",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_CommentHistory_CommentId_EditedAt",
                schema: "work",
                table: "CommentHistory");

            migrationBuilder.DropIndex(
                name: "IX_CommentHistory_EditedBy",
                schema: "work",
                table: "CommentHistory");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "EditedAt",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Pinned",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Resolved",
                schema: "work",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "EditedBy",
                schema: "work",
                table: "CommentHistory");

            migrationBuilder.DropColumn(
                name: "NewMessage",
                schema: "work",
                table: "CommentHistory");

            migrationBuilder.RenameColumn(
                name: "Message",
                schema: "work",
                table: "Comments",
                newName: "BodyMarkdown");

            migrationBuilder.RenameColumn(
                name: "PreviousMessage",
                schema: "work",
                table: "CommentHistory",
                newName: "PreviousBodyMarkdown");

            migrationBuilder.RenameColumn(
                name: "EditedByName",
                schema: "work",
                table: "CommentHistory",
                newName: "ChangedBy");

            migrationBuilder.RenameColumn(
                name: "EditedAt",
                schema: "work",
                table: "CommentHistory",
                newName: "ChangedOn");

            migrationBuilder.CreateIndex(
                name: "IX_CommentHistory_CommentId",
                schema: "work",
                table: "CommentHistory",
                column: "CommentId");
        }
    }
}
