using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProjectMembershipUserIdAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_UserProfiles_UserProfileId",
                schema: "project",
                table: "ProjectMembers");

            migrationBuilder.RenameColumn(
                name: "RoleInsideProject",
                schema: "project",
                table: "ProjectMembers",
                newName: "RoleInProject");

            migrationBuilder.RenameColumn(
                name: "AddedOn",
                schema: "project",
                table: "ProjectMembers",
                newName: "JoinedDate");

            migrationBuilder.RenameColumn(
                name: "UserProfileId",
                schema: "project",
                table: "ProjectMembers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMembers_UserProfileId",
                schema: "project",
                table: "ProjectMembers",
                newName: "IX_ProjectMembers_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMembers_RoleInsideProject",
                schema: "project",
                table: "ProjectMembers",
                newName: "IX_ProjectMembers_RoleInProject");

            migrationBuilder.Sql(
                """
                UPDATE members
                SET UserId = profiles.UserId
                FROM project.ProjectMembers AS members
                INNER JOIN organization.UserProfiles AS profiles
                    ON members.UserId = profiles.Id
                """);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "project",
                table: "ProjectMembers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_UserProfiles_UserId",
                schema: "organization",
                table: "UserProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMembers_IsActive",
                schema: "project",
                table: "ProjectMembers",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_UserProfiles_UserId",
                schema: "project",
                table: "ProjectMembers",
                column: "UserId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMembers_UserProfiles_UserId",
                schema: "project",
                table: "ProjectMembers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_UserProfiles_UserId",
                schema: "organization",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMembers_IsActive",
                schema: "project",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "project",
                table: "ProjectMembers");

            migrationBuilder.RenameColumn(
                name: "RoleInProject",
                schema: "project",
                table: "ProjectMembers",
                newName: "RoleInsideProject");

            migrationBuilder.RenameColumn(
                name: "JoinedDate",
                schema: "project",
                table: "ProjectMembers",
                newName: "AddedOn");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "project",
                table: "ProjectMembers",
                newName: "UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMembers_UserId",
                schema: "project",
                table: "ProjectMembers",
                newName: "IX_ProjectMembers_UserProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectMembers_RoleInProject",
                schema: "project",
                table: "ProjectMembers",
                newName: "IX_ProjectMembers_RoleInsideProject");

            migrationBuilder.Sql(
                """
                UPDATE members
                SET UserProfileId = profiles.Id
                FROM project.ProjectMembers AS members
                INNER JOIN organization.UserProfiles AS profiles
                    ON members.UserProfileId = profiles.UserId
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMembers_UserProfiles_UserProfileId",
                schema: "project",
                table: "ProjectMembers",
                column: "UserProfileId",
                principalSchema: "organization",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
