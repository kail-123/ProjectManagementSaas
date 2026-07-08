using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase3ClientProjectManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "project");

            migrationBuilder.CreateTable(
                name: "Clients",
                schema: "project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    GstNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Pan = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    State = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectManagerUserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Visibility = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
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
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Clients_ClientId",
                        column: x => x.ClientId,
                        principalSchema: "project",
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "organization",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_UserProfiles_ProjectManagerUserProfileId",
                        column: x => x.ProjectManagerUserProfileId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMembers",
                schema: "project",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleInsideProject = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMembers", x => new { x.ProjectId, x.UserProfileId });
                    table.ForeignKey(
                        name: "FK_ProjectMembers_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "project",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectMembers_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalSchema: "organization",
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSettings",
                schema: "project",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnableSprint = table.Column<bool>(type: "bit", nullable: false),
                    EnableBacklog = table.Column<bool>(type: "bit", nullable: false),
                    EnableKanban = table.Column<bool>(type: "bit", nullable: false),
                    EnableTimeTracking = table.Column<bool>(type: "bit", nullable: false),
                    EnableWiki = table.Column<bool>(type: "bit", nullable: false),
                    EnableDocuments = table.Column<bool>(type: "bit", nullable: false),
                    EnableApprovals = table.Column<bool>(type: "bit", nullable: false),
                    EnableLeaveRequests = table.Column<bool>(type: "bit", nullable: false),
                    EnableBugTracking = table.Column<bool>(type: "bit", nullable: false),
                    EnableRiskRegister = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSettings", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_ProjectSettings_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "project",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTeams",
                schema: "project",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AddedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTeams", x => new { x.ProjectId, x.TeamId });
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "project",
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "organization",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Code",
                schema: "project",
                table: "Clients",
                columns: new[] { "OrganizationId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_OrganizationId_Email",
                schema: "project",
                table: "Clients",
                columns: new[] { "OrganizationId", "Email" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status",
                schema: "project",
                table: "Clients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_RoleInsideProject",
                schema: "project",
                table: "ProjectMembers",
                column: "RoleInsideProject");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_UserProfileId",
                schema: "project",
                table: "ProjectMembers",
                column: "UserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId",
                schema: "project",
                table: "Projects",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_OrganizationId_Code",
                schema: "project",
                table: "Projects",
                columns: new[] { "OrganizationId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Priority",
                schema: "project",
                table: "Projects",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectManagerUserProfileId",
                schema: "project",
                table: "Projects",
                column: "ProjectManagerUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                schema: "project",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Visibility",
                schema: "project",
                table: "Projects",
                column: "Visibility");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTeams_TeamId",
                schema: "project",
                table: "ProjectTeams",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMembers",
                schema: "project");

            migrationBuilder.DropTable(
                name: "ProjectSettings",
                schema: "project");

            migrationBuilder.DropTable(
                name: "ProjectTeams",
                schema: "project");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "project");

            migrationBuilder.DropTable(
                name: "Clients",
                schema: "project");
        }
    }
}
