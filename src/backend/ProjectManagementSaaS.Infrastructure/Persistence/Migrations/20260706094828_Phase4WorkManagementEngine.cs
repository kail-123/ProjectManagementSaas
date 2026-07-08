using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase4WorkManagementEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "work");

            migrationBuilder.CreateTable(
                name: "Components",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Components_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "project",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Labels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Labels_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Priorities",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Priorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedFilters",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    QueryJson = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: false),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_SavedFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedFilters_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "project",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStatuses",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsInitial = table.Column<bool>(type: "bit", nullable: false),
                    IsTerminal = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_WorkflowStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStatuses_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "work",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemTypes",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_WorkItemTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemTypes_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "work",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowStatuses_FromStatusId",
                        column: x => x.FromStatusId,
                        principalSchema: "work",
                        principalTable: "WorkflowStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowStatuses_ToStatusId",
                        column: x => x.ToStatusId,
                        principalSchema: "work",
                        principalTable: "WorkflowStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "work",
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItems",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentWorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WorkItemTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: true),
                    AcceptanceCriteria = table.Column<string>(type: "nvarchar(max)", maxLength: 4096, nullable: true),
                    AssigneeUserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReporterUserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    LoggedHours = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    StoryPoints = table.Column<int>(type: "int", nullable: true),
                    SprintId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Environment = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Reproducible = table.Column<bool>(type: "bit", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("PK_WorkItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItems_Priorities_PriorityId",
                        column: x => x.PriorityId,
                        principalSchema: "work",
                        principalTable: "Priorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "project",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_UserProfiles_AssigneeUserProfileId",
                        column: x => x.AssigneeUserProfileId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_UserProfiles_ReporterUserProfileId",
                        column: x => x.ReporterUserProfileId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_WorkItemTypes_WorkItemTypeId",
                        column: x => x.WorkItemTypeId,
                        principalSchema: "work",
                        principalTable: "WorkItemTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_WorkItems_ParentWorkItemId",
                        column: x => x.ParentWorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItems_WorkflowStatuses_WorkflowStatusId",
                        column: x => x.WorkflowStatusId,
                        principalSchema: "work",
                        principalTable: "WorkflowStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTimeline",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTimeline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTimeline_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BodyMarkdown = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalSchema: "work",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemComponents",
                schema: "work",
                columns: table => new
                {
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ComponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemComponents", x => new { x.WorkItemId, x.ComponentId });
                    table.ForeignKey(
                        name: "FK_WorkItemComponents_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalSchema: "work",
                        principalTable: "Components",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemComponents_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemLabels",
                schema: "work",
                columns: table => new
                {
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemLabels", x => new { x.WorkItemId, x.LabelId });
                    table.ForeignKey(
                        name: "FK_WorkItemLabels_Labels_LabelId",
                        column: x => x.LabelId,
                        principalSchema: "work",
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemLabels_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemLinks",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceWorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetWorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
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
                    table.PrimaryKey("PK_WorkItemLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemLinks_WorkItems_SourceWorkItemId",
                        column: x => x.SourceWorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemLinks_WorkItems_TargetWorkItemId",
                        column: x => x.TargetWorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkItemWatchers",
                schema: "work",
                columns: table => new
                {
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemWatchers", x => new { x.WorkItemId, x.UserProfileId });
                    table.ForeignKey(
                        name: "FK_WorkItemWatchers_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemWatchers_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalSchema: "work",
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentHistory",
                schema: "work",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousBodyMarkdown = table.Column<string>(type: "nvarchar(max)", maxLength: 8192, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentHistory_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "work",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentMentions",
                schema: "work",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentMentions", x => new { x.CommentId, x.UserProfileId });
                    table.ForeignKey(
                        name: "FK_CommentMentions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "work",
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentMentions_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTimeline_WorkItemId_CreatedOn",
                schema: "work",
                table: "ActivityTimeline",
                columns: new[] { "WorkItemId", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_WorkItemId_Version_FileName",
                schema: "work",
                table: "Attachments",
                columns: new[] { "WorkItemId", "Version", "FileName" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentHistory_CommentId",
                schema: "work",
                table: "CommentHistory",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMentions_UserProfileId",
                schema: "work",
                table: "CommentMentions",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "work",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WorkItemId",
                schema: "work",
                table: "Comments",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Components_ProjectId_Name",
                schema: "work",
                table: "Components",
                columns: new[] { "ProjectId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Labels_OrganizationId_Name",
                schema: "work",
                table: "Labels",
                columns: new[] { "OrganizationId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_Code",
                schema: "work",
                table: "Priorities",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_SortOrder",
                schema: "work",
                table: "Priorities",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SavedFilters_OwnerUserId_Name",
                schema: "work",
                table: "SavedFilters",
                columns: new[] { "OwnerUserId", "Name" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SavedFilters_ProjectId",
                schema: "work",
                table: "SavedFilters",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_Code",
                schema: "work",
                table: "Workflows",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_WorkflowId_Code",
                schema: "work",
                table: "WorkflowStatuses",
                columns: new[] { "WorkflowId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStatuses_WorkflowId_SortOrder",
                schema: "work",
                table: "WorkflowStatuses",
                columns: new[] { "WorkflowId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_FromStatusId",
                schema: "work",
                table: "WorkflowTransitions",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_ToStatusId",
                schema: "work",
                table: "WorkflowTransitions",
                column: "ToStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowId_FromStatusId_ToStatusId",
                schema: "work",
                table: "WorkflowTransitions",
                columns: new[] { "WorkflowId", "FromStatusId", "ToStatusId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemComponents_ComponentId",
                schema: "work",
                table: "WorkItemComponents",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLabels_LabelId",
                schema: "work",
                table: "WorkItemLabels",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLinks_SourceWorkItemId_TargetWorkItemId_LinkType",
                schema: "work",
                table: "WorkItemLinks",
                columns: new[] { "SourceWorkItemId", "TargetWorkItemId", "LinkType" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLinks_TargetWorkItemId",
                schema: "work",
                table: "WorkItemLinks",
                column: "TargetWorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_AssigneeUserProfileId",
                schema: "work",
                table: "WorkItems",
                column: "AssigneeUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_DueDate",
                schema: "work",
                table: "WorkItems",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ParentWorkItemId",
                schema: "work",
                table: "WorkItems",
                column: "ParentWorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_PriorityId",
                schema: "work",
                table: "WorkItems",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId",
                schema: "work",
                table: "WorkItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ReporterUserProfileId",
                schema: "work",
                table: "WorkItems",
                column: "ReporterUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_WorkflowStatusId",
                schema: "work",
                table: "WorkItems",
                column: "WorkflowStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_WorkItemTypeId",
                schema: "work",
                table: "WorkItems",
                column: "WorkItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTypes_Code",
                schema: "work",
                table: "WorkItemTypes",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemTypes_WorkflowId",
                schema: "work",
                table: "WorkItemTypes",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemWatchers_UserProfileId",
                schema: "work",
                table: "WorkItemWatchers",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityTimeline",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Attachments",
                schema: "work");

            migrationBuilder.DropTable(
                name: "CommentHistory",
                schema: "work");

            migrationBuilder.DropTable(
                name: "CommentMentions",
                schema: "work");

            migrationBuilder.DropTable(
                name: "SavedFilters",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkItemComponents",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkItemLabels",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkItemLinks",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkItemWatchers",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Components",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Labels",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkItems",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Priorities",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkItemTypes",
                schema: "work");

            migrationBuilder.DropTable(
                name: "WorkflowStatuses",
                schema: "work");

            migrationBuilder.DropTable(
                name: "Workflows",
                schema: "work");
        }
    }
}
