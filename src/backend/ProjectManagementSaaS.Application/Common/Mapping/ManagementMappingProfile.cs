using AutoMapper;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;
using ProjectManagementSaaS.Application.Features.Projects.Contracts;
using ProjectManagementSaaS.Application.Features.Security.Contracts;
using ProjectManagementSaaS.Application.Features.Users.Contracts;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Domain.Projects;
using ProjectManagementSaaS.Domain.Security;

namespace ProjectManagementSaaS.Application.Common.Mapping;

public sealed class ManagementMappingProfile : Profile
{
    public ManagementMappingProfile()
    {
        CreateMap<Organization, OrganizationDto>();
        CreateMap<OrganizationUpsertRequest, Organization>();

        CreateMap<Department, DepartmentDto>()
            .ForCtorParam(nameof(DepartmentDto.OrganizationName), options => options.MapFrom(source => source.Organization.Name));
        CreateMap<DepartmentUpsertRequest, Department>();

        CreateMap<UserProfile, UserProfileDto>()
            .ForCtorParam(nameof(UserProfileDto.OrganizationName), options => options.MapFrom(source => source.Organization == null ? null : source.Organization.Name))
            .ForCtorParam(nameof(UserProfileDto.DepartmentName), options => options.MapFrom(source => source.Department == null ? null : source.Department.Name))
            .ForCtorParam(nameof(UserProfileDto.TeamName), options => options.MapFrom(source => source.Team == null ? null : source.Team.Name));

        CreateMap<Team, TeamDto>()
            .ForCtorParam(nameof(TeamDto.DepartmentName), options => options.MapFrom(source => source.Department.Name))
            .ForCtorParam(nameof(TeamDto.TeamLeadName), options => options.MapFrom(source => source.TeamLead == null ? null : source.TeamLead.FirstName + " " + source.TeamLead.LastName))
            .ForCtorParam(nameof(TeamDto.MemberUserProfileIds), options => options.MapFrom(source => source.Members.Select(member => member.UserProfileId).ToArray()));
        CreateMap<TeamUpsertRequest, Team>();

        CreateMap<Client, ClientDto>()
            .ForCtorParam(nameof(ClientDto.OrganizationName), options => options.MapFrom(source => source.Organization.Name));
        CreateMap<ClientUpsertRequest, Client>();

        CreateMap<Project, ProjectDto>()
            .ForCtorParam(nameof(ProjectDto.OrganizationName), options => options.MapFrom(source => source.Organization.Name))
            .ForCtorParam(nameof(ProjectDto.ClientName), options => options.MapFrom(source => source.Client == null ? null : source.Client.Name))
            .ForCtorParam(nameof(ProjectDto.ProjectManagerName), options => options.MapFrom(source => source.ProjectManager == null ? null : source.ProjectManager.FirstName + " " + source.ProjectManager.LastName))
            .ForCtorParam(nameof(ProjectDto.TeamIds), options => options.MapFrom(source => source.ProjectTeams.Select(projectTeam => projectTeam.TeamId).ToArray()))
            .ForCtorParam(nameof(ProjectDto.MemberCount), options => options.MapFrom(source => source.Members.Count(member => member.IsActive)));
        CreateMap<ProjectUpsertRequest, Project>()
            .ForMember(destination => destination.ProjectTeams, options => options.Ignore())
            .ForMember(destination => destination.Members, options => options.Ignore())
            .ForMember(destination => destination.Settings, options => options.Ignore());

        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForCtorParam(nameof(ProjectMemberDto.Id), options => options.MapFrom(source => source.UserId))
            .ForCtorParam(nameof(ProjectMemberDto.UserProfileId), options => options.MapFrom(source => source.UserProfile == null ? null : (Guid?)source.UserProfile.Id))
            .ForCtorParam(nameof(ProjectMemberDto.UserDisplayName), options => options.MapFrom(source => source.UserProfile == null ? source.UserId.ToString() : source.UserProfile.FirstName + " " + source.UserProfile.LastName))
            .ForCtorParam(nameof(ProjectMemberDto.EmployeeCode), options => options.MapFrom(source => source.UserProfile == null ? string.Empty : source.UserProfile.EmployeeCode))
            .ForCtorParam(nameof(ProjectMemberDto.Designation), options => options.MapFrom(source => source.UserProfile == null ? null : source.UserProfile.Designation));

        CreateMap<ProjectSettings, ProjectSettingsDto>();

        CreateMap<Permission, PermissionDto>();
        CreateMap<PermissionUpsertRequest, Permission>();
    }
}
