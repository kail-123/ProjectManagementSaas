using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Organizations.Contracts;
using ProjectManagementSaaS.Domain.Organizations;

namespace ProjectManagementSaaS.Application.Features.Organizations;

public sealed record ListTeamsQuery(PagedRequest Request, Guid? DepartmentId) : IRequest<PagedResult<TeamDto>>;

public sealed record GetTeamByIdQuery(Guid Id) : IRequest<TeamDto>;

public sealed record CreateTeamCommand(TeamUpsertRequest Request) : IRequest<TeamDto>;

public sealed record UpdateTeamCommand(Guid Id, TeamUpsertRequest Request) : IRequest<TeamDto>;

public sealed record DeleteTeamCommand(Guid Id) : IRequest;

public sealed record BulkDeleteTeamsCommand(BulkDeleteRequest Request) : IRequest;

internal sealed class ListTeamsQueryHandler(
    IRepository<Team> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<ListTeamsQuery, PagedResult<TeamDto>>
{
    public Task<PagedResult<TeamDto>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = repository.Query();

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(team => team.DepartmentId == request.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var term = request.Request.Search.Trim();
            query = query.Where(team =>
                team.Name.Contains(term)
                || team.Code.Contains(term)
                || team.Department.Name.Contains(term));
        }

        var descending = request.Request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = (request.Request.SortBy?.ToLowerInvariant(), descending) switch
        {
            ("code", true) => query.OrderByDescending(team => team.Code),
            ("code", false) => query.OrderBy(team => team.Code),
            ("department", true) => query.OrderByDescending(team => team.Department.Name),
            ("department", false) => query.OrderBy(team => team.Department.Name),
            ("createdon", true) => query.OrderByDescending(team => team.CreatedOn),
            ("createdon", false) => query.OrderBy(team => team.CreatedOn),
            ("name", true) => query.OrderByDescending(team => team.Name),
            _ => query.OrderBy(team => team.Name)
        };

        return paginationService.CreateAsync(
            query.ProjectTo<TeamDto>(mapper.ConfigurationProvider),
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);
    }
}

internal sealed class GetTeamByIdQueryHandler(
    IRepository<Team> repository,
    IPaginationService paginationService,
    IMapper mapper)
    : IRequestHandler<GetTeamByIdQuery, TeamDto>
{
    public async Task<TeamDto> Handle(GetTeamByIdQuery request, CancellationToken cancellationToken)
    {
        var team = await paginationService.SingleOrDefaultAsync(
            repository.Query()
                .Where(entity => entity.Id == request.Id)
                .ProjectTo<TeamDto>(mapper.ConfigurationProvider),
            cancellationToken);

        return team ?? throw new ApplicationNotFoundException("Team was not found.");
    }
}

internal sealed class CreateTeamCommandHandler(
    IRepository<Department> departmentRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<Team> repository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.Request.DepartmentId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Department was not found.");

        if (request.Request.TeamLeadUserProfileId.HasValue)
        {
            _ = await userProfileRepository.GetByIdAsync(request.Request.TeamLeadUserProfileId.Value, cancellationToken)
                ?? throw new ApplicationNotFoundException("Team lead user profile was not found.");
        }

        var team = mapper.Map<Team>(request.Request);
        team.Id = Guid.NewGuid();
        team.Department = department;
        ReplaceMembers(team, request.Request.MemberUserProfileIds, dateTimeProvider.UtcNow, currentUserService.UserName);

        await repository.AddAsync(team, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<TeamDto>(team);
    }

    private static void ReplaceMembers(Team team, IEnumerable<Guid> userProfileIds, DateTimeOffset addedOn, string? addedBy)
    {
        team.Members.Clear();
        foreach (var userProfileId in userProfileIds.Distinct())
        {
            team.Members.Add(new TeamMember
            {
                TeamId = team.Id,
                UserProfileId = userProfileId,
                AddedOn = addedOn,
                AddedBy = addedBy
            });
        }
    }
}

internal sealed class UpdateTeamCommandHandler(
    IRepository<Department> departmentRepository,
    IRepository<UserProfile> userProfileRepository,
    IRepository<Team> repository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<UpdateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        var department = await departmentRepository.GetByIdAsync(request.Request.DepartmentId, cancellationToken)
            ?? throw new ApplicationNotFoundException("Department was not found.");

        if (request.Request.TeamLeadUserProfileId.HasValue)
        {
            _ = await userProfileRepository.GetByIdAsync(request.Request.TeamLeadUserProfileId.Value, cancellationToken)
                ?? throw new ApplicationNotFoundException("Team lead user profile was not found.");
        }

        var team = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Team was not found.");

        mapper.Map(request.Request, team);
        team.Department = department;
        team.Members.Clear();
        foreach (var userProfileId in request.Request.MemberUserProfileIds.Distinct())
        {
            team.Members.Add(new TeamMember
            {
                TeamId = team.Id,
                UserProfileId = userProfileId,
                AddedOn = dateTimeProvider.UtcNow,
                AddedBy = currentUserService.UserName
            });
        }

        repository.Update(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<TeamDto>(team);
    }
}

internal sealed class DeleteTeamCommandHandler(IRepository<Team> repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteTeamCommand>
{
    public async Task Handle(DeleteTeamCommand request, CancellationToken cancellationToken)
    {
        var team = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ApplicationNotFoundException("Team was not found.");

        repository.Remove(team);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class BulkDeleteTeamsCommandHandler(
    IRepository<Team> repository,
    IPaginationService paginationService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<BulkDeleteTeamsCommand>
{
    public async Task Handle(BulkDeleteTeamsCommand request, CancellationToken cancellationToken)
    {
        var teams = await paginationService.ToListAsync(
            repository.Query().Where(team => request.Request.Ids.Contains(team.Id)),
            cancellationToken);

        repository.RemoveRange(teams);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
