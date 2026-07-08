using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActivityLogEnterpriseTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityTimeline_WorkItems_WorkItemId",
                schema: "work",
                table: "ActivityTimeline");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityTimeline",
                schema: "work",
                table: "ActivityTimeline");

            migrationBuilder.RenameTable(
                name: "ActivityTimeline",
                schema: "work",
                newName: "ActivityLogs",
                newSchema: "work");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                schema: "work",
                table: "ActivityLogs",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityTimeline_WorkItemId_CreatedOn",
                schema: "work",
                table: "ActivityLogs",
                newName: "IX_ActivityLogs_WorkItemId_CreatedAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkItemId",
                schema: "work",
                table: "ActivityLogs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "work",
                table: "ActivityLogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "System");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "work",
                table: "ActivityLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "work",
                table: "ActivityLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE activity
                SET
                    activity.ProjectId = workItem.ProjectId,
                    activity.Category = CASE
                        WHEN activity.ActivityType IN ('CommentAdded', 'ReplyAdded', 'CommentEdited', 'CommentUpdated', 'CommentDeleted') THEN 'Comments'
                        WHEN activity.ActivityType IN ('AssignmentChanged', 'ReporterChanged', 'WatcherAdded', 'WatcherRemoved') THEN 'Assignments'
                        WHEN activity.ActivityType IN ('StatusChanged', 'WorkflowTransitioned', 'Resolved', 'Reopened') THEN 'StatusChanges'
                        WHEN activity.ActivityType = 'PriorityChanged' THEN 'Priority'
                        WHEN activity.ActivityType IN ('AttachmentAdded', 'AttachmentUpdated', 'AttachmentDeleted') THEN 'Attachments'
                        WHEN activity.ActivityType IN ('MentionAdded', 'MentionRemoved') THEN 'Mentions'
                        WHEN activity.ActivityType IN ('ReactionAdded', 'ReactionRemoved') THEN 'Reactions'
                        WHEN activity.ActivityType = 'ProjectMembershipChanged' THEN 'ProjectMembership'
                        ELSE 'System'
                    END
                FROM [work].[ActivityLogs] AS activity
                INNER JOIN [work].[WorkItems] AS workItem ON workItem.Id = activity.WorkItemId
                WHERE activity.ProjectId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                schema: "work",
                table: "ActivityLogs",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityLogs",
                schema: "work",
                table: "ActivityLogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_Category_CreatedAt",
                schema: "work",
                table: "ActivityLogs",
                columns: new[] { "Category", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ProjectId_CreatedAt",
                schema: "work",
                table: "ActivityLogs",
                columns: new[] { "ProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_UserId",
                schema: "work",
                table: "ActivityLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Projects_ProjectId",
                schema: "work",
                table: "ActivityLogs",
                column: "ProjectId",
                principalSchema: "project",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_UserProfiles_UserId",
                schema: "work",
                table: "ActivityLogs",
                column: "UserId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_WorkItems_WorkItemId",
                schema: "work",
                table: "ActivityLogs",
                column: "WorkItemId",
                principalSchema: "work",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_Projects_ProjectId",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_UserProfiles_UserId",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLogs_WorkItems_WorkItemId",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActivityLogs",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_Category_CreatedAt",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_ProjectId_CreatedAt",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropIndex(
                name: "IX_ActivityLogs_UserId",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.Sql("DELETE FROM [work].[ActivityLogs] WHERE [WorkItemId] IS NULL;");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "work",
                table: "ActivityLogs");

            migrationBuilder.RenameTable(
                name: "ActivityLogs",
                schema: "work",
                newName: "ActivityTimeline",
                newSchema: "work");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "work",
                table: "ActivityTimeline",
                newName: "CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_ActivityLogs_WorkItemId_CreatedAt",
                schema: "work",
                table: "ActivityTimeline",
                newName: "IX_ActivityTimeline_WorkItemId_CreatedOn");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkItemId",
                schema: "work",
                table: "ActivityTimeline",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActivityTimeline",
                schema: "work",
                table: "ActivityTimeline",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityTimeline_WorkItems_WorkItemId",
                schema: "work",
                table: "ActivityTimeline",
                column: "WorkItemId",
                principalSchema: "work",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
