using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Abstractions.Common;
using ProjectManagementSaaS.Application.Abstractions.Identity;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Models;
using ProjectManagementSaaS.Application.Features.Users.Contracts;
using ProjectManagementSaaS.Application.Features.Work.Contracts;
using ProjectManagementSaaS.Domain.Organizations;
using ProjectManagementSaaS.Infrastructure.Persistence;
using ProjectManagementSaaS.Infrastructure.Persistence.Entities;

namespace ProjectManagementSaaS.Infrastructure.Identity;

internal sealed class UserManagementService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IUserPermissionService userPermissionService)
    : IUserManagementService
{
    public async Task<PagedResult<UserDto>> ListAsync(PagedRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .GroupJoin(
                dbContext.UserProfiles,
                user => user.Id,
                profile => profile.UserId,
                (user, profiles) => new { User = user, Profile = profiles.FirstOrDefault() });

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(row =>
                row.User.Email != null && row.User.Email.Contains(term)
                || row.User.DisplayName != null && row.User.DisplayName.Contains(term)
                || row.Profile != null && (row.Profile.FirstName.Contains(term)
                    || row.Profile.LastName.Contains(term)
                    || row.Profile.EmployeeCode.Contains(term)));
        }

        var descending = request.SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        query = (request.SortBy?.ToLowerInvariant(), descending) switch
        {
            ("email", true) => query.OrderByDescending(row => row.User.Email),
            ("email", false) => query.OrderBy(row => row.User.Email),
            ("employeecode", true) => query.OrderByDescending(row => row.Profile == null ? null : row.Profile.EmployeeCode),
            ("employeecode", false) => query.OrderBy(row => row.Profile == null ? null : row.Profile.EmployeeCode),
            ("name", true) => query.OrderByDescending(row => row.User.DisplayName),
            _ => query.OrderBy(row => row.User.DisplayName).ThenBy(row => row.User.Email)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(row => row.User)
            .ToListAsync(cancellationToken);

        var items = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            items.Add(await MapUserAsync(user, cancellationToken));
        }

        return new PagedResult<UserDto>(items, request.PageNumber, request.PageSize, totalCount);
    }

    public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new ApplicationNotFoundException("User was not found.");

        return await MapUserAsync(user, cancellationToken);
    }

    public async Task<PagedResult<WorkMentionCandidateDto>> ListProjectMentionCandidatesAsync(Guid projectId, PagedRequest request, CancellationToken cancellationToken)
    {
        var query = dbContext.ProjectMembers
            .Where(member => member.ProjectId == projectId && member.IsActive && member.UserProfile != null && member.UserProfile.IsActive)
            .Join(
                dbContext.Users.Where(user => user.IsActive),
                member => member.UserId,
                user => user.Id,
                (member, user) => new { Member = member, User = user });

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(row =>
                row.User.Email != null && row.User.Email.Contains(term)
                || row.User.DisplayName != null && row.User.DisplayName.Contains(term)
                || row.Member.UserProfile != null && (row.Member.UserProfile.FirstName.Contains(term)
                    || row.Member.UserProfile.LastName.Contains(term)
                    || row.Member.UserProfile.EmployeeCode.Contains(term)));
        }

        query = query
            .OrderBy(row => row.Member.UserProfile == null ? row.User.DisplayName : row.Member.UserProfile.FirstName)
            .ThenBy(row => row.Member.UserProfile == null ? row.User.Email : row.Member.UserProfile.LastName);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(row => new WorkMentionCandidateDto(
                row.User.Id,
                row.Member.UserProfile!.Id,
                row.Member.UserProfile.FirstName + " " + row.Member.UserProfile.LastName,
                row.Member.UserProfile.EmployeeCode,
                row.User.Email ?? string.Empty,
                row.Member.UserProfile.Designation,
                row.Member.UserProfile.Department == null ? null : row.Member.UserProfile.Department.Name,
                row.Member.UserProfile.ProfilePhoto))
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkMentionCandidateDto>(items, request.PageNumber, request.PageSize, totalCount);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await ValidateAssignmentsAsync(request.OrganizationId, request.DepartmentId, request.TeamId, cancellationToken);

        var email = request.Email.Trim();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            DisplayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}",
            IsActive = request.IsActive,
            CreatedAtUtc = dateTimeProvider.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        ThrowIfFailed(result, "Unable to create user.");

        dbContext.UserProfiles.Add(CreateProfile(user.Id, request));
        await dbContext.SaveChangesAsync(cancellationToken);

        await AssignRolesAsync(user.Id, request.Roles, cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<UserDto> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await ValidateAssignmentsAsync(request.OrganizationId, request.DepartmentId, request.TeamId, cancellationToken);

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new ApplicationNotFoundException("User was not found.");

        user.Email = request.Email.Trim();
        user.UserName = request.Email.Trim();
        user.DisplayName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";
        user.IsActive = request.IsActive;

        var result = await userManager.UpdateAsync(user);
        ThrowIfFailed(result, "Unable to update user.");

        var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(entity => entity.UserId == user.Id, cancellationToken);
        if (profile is null)
        {
            dbContext.UserProfiles.Add(CreateProfile(user.Id, request));
        }
        else
        {
            ApplyProfile(profile, request);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task SetActiveStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new ApplicationNotFoundException("User was not found.");

        user.IsActive = isActive;
        var profile = await dbContext.UserProfiles.SingleOrDefaultAsync(entity => entity.UserId == user.Id, cancellationToken);
        if (profile is not null)
        {
            profile.IsActive = isActive;
        }

        var result = await userManager.UpdateAsync(user);
        ThrowIfFailed(result, "Unable to update user active status.");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (currentUserService.UserId == userId)
        {
            throw new ApplicationConflictException("The current signed-in user cannot be deleted.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new ApplicationNotFoundException("User was not found.");

        await EnsureUserProfileCanBeDeletedAsync(user.Id, cancellationToken);

        var result = await userManager.DeleteAsync(user);
        ThrowIfFailed(result, "Unable to delete user.");
    }

    public async Task BulkDeleteAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken)
    {
        foreach (var userId in userIds.Distinct())
        {
            await DeleteAsync(userId, cancellationToken);
        }
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new ApplicationNotFoundException("User was not found.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
        ThrowIfFailed(result, "Unable to reset password.");
    }

    public async Task AssignRolesAsync(Guid userId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new ApplicationNotFoundException("User was not found.");

        var distinctRoles = roleNames.Select(role => role.Trim()).Where(role => role.Length > 0).Distinct(StringComparer.Ordinal).ToArray();
        var currentRoles = await userManager.GetRolesAsync(user);

        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles.Except(distinctRoles, StringComparer.Ordinal));
        ThrowIfFailed(removeResult, "Unable to remove user roles.");

        var addResult = await userManager.AddToRolesAsync(user, distinctRoles.Except(currentRoles, StringComparer.Ordinal));
        ThrowIfFailed(addResult, "Unable to assign user roles.");
        userPermissionService.InvalidateUser(user.Id);
    }

    private async Task<UserDto> MapUserAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var profile = await dbContext.UserProfiles
            .Include(entity => entity.Organization)
            .Include(entity => entity.Department)
            .Include(entity => entity.Team)
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.UserId == user.Id, cancellationToken);

        return new UserDto(user.Id, user.Email ?? string.Empty, user.DisplayName, user.IsActive, roles.ToArray(), MapProfile(profile));
    }

    private static UserProfileDto? MapProfile(UserProfile? profile)
    {
        return profile is null
            ? null
            : new UserProfileDto(
                profile.Id,
                profile.UserId,
                profile.OrganizationId,
                profile.Organization?.Name,
                profile.DepartmentId,
                profile.Department?.Name,
                profile.TeamId,
                profile.Team?.Name,
                profile.FirstName,
                profile.LastName,
                profile.EmployeeCode,
                profile.Designation,
                profile.ProfilePhoto,
                profile.Phone,
                profile.TimeZone,
                profile.Skills,
                profile.JoiningDate,
                profile.IsActive);
    }

    private static UserProfile CreateProfile(Guid userId, CreateUserRequest request)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = request.OrganizationId,
            DepartmentId = request.DepartmentId,
            TeamId = request.TeamId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmployeeCode = request.EmployeeCode.Trim(),
            Designation = request.Designation,
            ProfilePhoto = request.ProfilePhoto,
            Phone = request.Phone,
            TimeZone = request.TimeZone,
            Skills = request.Skills,
            JoiningDate = request.JoiningDate,
            IsActive = request.IsActive
        };
    }

    private static UserProfile CreateProfile(Guid userId, UpdateUserRequest request)
    {
        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = request.OrganizationId,
            DepartmentId = request.DepartmentId,
            TeamId = request.TeamId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmployeeCode = request.EmployeeCode.Trim(),
            Designation = request.Designation,
            ProfilePhoto = request.ProfilePhoto,
            Phone = request.Phone,
            TimeZone = request.TimeZone,
            Skills = request.Skills,
            JoiningDate = request.JoiningDate,
            IsActive = request.IsActive
        };
    }

    private static void ApplyProfile(UserProfile profile, UpdateUserRequest request)
    {
        profile.OrganizationId = request.OrganizationId;
        profile.DepartmentId = request.DepartmentId;
        profile.TeamId = request.TeamId;
        profile.FirstName = request.FirstName.Trim();
        profile.LastName = request.LastName.Trim();
        profile.EmployeeCode = request.EmployeeCode.Trim();
        profile.Designation = request.Designation;
        profile.ProfilePhoto = request.ProfilePhoto;
        profile.Phone = request.Phone;
        profile.TimeZone = request.TimeZone;
        profile.Skills = request.Skills;
        profile.JoiningDate = request.JoiningDate;
        profile.IsActive = request.IsActive;
    }

    private async Task ValidateAssignmentsAsync(Guid? organizationId, Guid? departmentId, Guid? teamId, CancellationToken cancellationToken)
    {
        if (organizationId.HasValue && !await dbContext.Organizations.AnyAsync(entity => entity.Id == organizationId.Value, cancellationToken))
        {
            throw new ApplicationNotFoundException("Organization was not found.");
        }

        if (departmentId.HasValue && !await dbContext.Departments.AnyAsync(entity => entity.Id == departmentId.Value, cancellationToken))
        {
            throw new ApplicationNotFoundException("Department was not found.");
        }

        if (teamId.HasValue && !await dbContext.Teams.AnyAsync(entity => entity.Id == teamId.Value, cancellationToken))
        {
            throw new ApplicationNotFoundException("Team was not found.");
        }
    }

    private async Task EnsureUserProfileCanBeDeletedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profileId = await dbContext.UserProfiles
            .IgnoreQueryFilters()
            .Where(profile => profile.UserId == userId)
            .Select(profile => (Guid?)profile.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (!profileId.HasValue)
        {
            return;
        }

        var hasTeamReference = await dbContext.Teams
            .IgnoreQueryFilters()
            .AnyAsync(team => team.TeamLeadUserProfileId == profileId.Value, cancellationToken)
            || await dbContext.TeamMembers
                .IgnoreQueryFilters()
                .AnyAsync(member => member.UserProfileId == profileId.Value, cancellationToken);

        var hasProjectReference = await dbContext.Projects
            .IgnoreQueryFilters()
            .AnyAsync(project => project.ProjectManagerUserProfileId == profileId.Value, cancellationToken)
            || await dbContext.ProjectMembers
                .IgnoreQueryFilters()
                .AnyAsync(member => member.UserId == userId, cancellationToken);

        if (hasTeamReference || hasProjectReference)
        {
            throw new ApplicationConflictException("User cannot be deleted while assigned to teams or projects. Deactivate the user instead.");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new ApplicationConflictException($"{message} {errors}");
    }
}
