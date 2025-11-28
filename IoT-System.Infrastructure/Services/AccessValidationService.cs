using System.Security.Claims;
using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Services;

public class AccessValidationService : IAccessValidationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IGroupRepository _groupRepository;

    // Role hierarchy: lower number = higher priority (0 = highest)
    private static readonly Dictionary<string, int> RolePriority = new()
    {
        { Constants.Roles.SuperAdmin, 0 },
        { Constants.Roles.Admin, 10 },
        { Constants.Roles.Scientist, 20 },
        { Constants.Roles.Viewer, 30 }
    };

    public AccessValidationService(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IGroupRepository groupRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _groupRepository = groupRepository;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? throw new UnauthorizedAccessException("No user context available");

    #region Current User Information

    public Guid GetCurrentUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.UserId)?.Value;
        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User ID not found in token");
    }

    public string GetCurrentUserName()
    {
        return User.Claims.FirstOrDefault(c => c.Type == Constants.ClaimTypes.UserName)?.Value ??
               throw new UnauthorizedAccessException("Username not found in token");
    }

    public List<Guid> GetCurrentUserRolesIds()
    {
        return User.Claims
            .Where(c => c.Type == Constants.ClaimTypes.RoleId)
            .Select(c => Guid.Parse(c.Value))
            .ToList();
    }

    public List<string> GetCurrentUserRolesNames()
    {
        return User.Claims
            .Where(c => c.Type == Constants.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    public Task<OperationResult<List<Role>>> GetCurrentUserRoles()
    {
        return ExecuteAsync(() =>
        {
            var userId = GetCurrentUserId();
            var roleIds = GetCurrentUserRolesIds();
            var roleNames = GetCurrentUserRolesNames();

            return _roleRepository.AsQueryable()
                .Where(r => (roleIds.Contains(r.Id) || roleNames.Contains(r.Name))
                            && r.UserRoles.Any(ur => ur.UserId == userId)
                ).ToListAsync();
        });
    }

    public List<Guid> GetCurrentUserGroupsIds()
    {
        return User.Claims
            .Where(c => c.Type == Constants.ClaimTypes.GroupId)
            .Select(c => Guid.Parse(c.Value))
            .ToList();
    }

    public List<string> GetCurrentUserGroupsNames()
    {
        return User.Claims
            .Where(c => c.Type == Constants.ClaimTypes.GroupName)
            .Select(c => c.Value)
            .ToList();
    }

    public Task<OperationResult<List<Group>>> GetCurrentUserGroups()
    {
        return ExecuteAsync(() =>
        {
            var userId = GetCurrentUserId();
            var groupIds = GetCurrentUserGroupsIds();
            var groupNames = GetCurrentUserGroupsNames();

            return _groupRepository.AsQueryable()
                .Where(g => (groupIds.Contains(g.Id) || groupNames.Contains(g.Name))
                            && g.UserGroups.Any(ug => ug.UserId == userId)
                ).ToListAsync();
        });
    }

    public Task<OperationResult<User>> GetCurrentUserAsync(bool includeRolesAndGroups = false)
    {
        return ExecuteAsync(async Task<User> () =>
        {
            var userId = GetCurrentUserId();
            var userQuery = _userRepository.AsQueryable();

            if (includeRolesAndGroups)
            {
                userQuery = userQuery
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                    .ThenInclude(g => g.GroupRoles)
                    .ThenInclude(gr => gr.Role)
                    .AsSplitQuery();
            }

            var user = await userQuery.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return OperationResult<User>.NotFound($"Current user with ID {userId} not found")!;
            }

            return user;
        });
    }

    #endregion

    #region Role Validation

    public async Task<OperationResult> ValidateRolesAccessAsync(List<Guid> roleIds)
    {
        if (!roleIds.Any()) return OperationResult.Success();

        var currentUserRolesIds = GetCurrentUserRolesIds();
        var rolesToValidate = roleIds.Except(currentUserRolesIds).ToList();
        if (!rolesToValidate.Any()) return OperationResult.Success();

        var rolesResult = await ExecuteAsync(() =>
            _roleRepository.AsQueryable()
                .Where(r => rolesToValidate.Contains(r.Id))
                .ToListAsync()
        );

        if (!rolesResult.IsSuccess) return rolesResult.ToOperationResult();
        var roles = rolesResult.Data!;

        if (roles.Count != rolesToValidate.Count)
        {
            var foundIds = roles.Select(r => r.Id).ToHashSet();
            var missingIds = rolesToValidate.Where(id => !foundIds.Contains(id)).ToList();
            return OperationResult.NotFound($"Roles not found: {string.Join(", ", missingIds)}");
        }

        return await ValidateRolesAccessAsync(roles);
    }

    public Task<OperationResult> ValidateRolesAccessAsync(List<string> roleNames)
    {
        if (!roleNames.Any()) return Task.FromResult(OperationResult.Success());

        var currentUserRoleNames = GetCurrentUserRolesNames();
        var rolesToValidate = roleNames.Except(currentUserRoleNames).ToList();
        if (!rolesToValidate.Any()) return Task.FromResult(OperationResult.Success());

        var currentUserPriority = GetCurrentUserHighestPriority();
        var targetRolesPriority = GetRolesPriority(rolesToValidate);

        // Current user must have higher or equal priority (lower or equal number)
        if (currentUserPriority > targetRolesPriority)
        {
            var highestTargetRole = rolesToValidate
                .OrderBy(GetRolePriority)
                .First();

            return Task.FromResult(OperationResult.Forbidden(
                $"You don't have sufficient permissions to manage role '{highestTargetRole}'. " +
                $"Only users with higher or equal role priority can manage this role."
            ));
        }

        return Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> ValidateRolesAccessAsync(List<Role> roles)
        => ValidateRolesAccessAsync(roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .ToList()
        );

    #endregion

    #region User Validation

    public async Task<OperationResult> ValidateUserAccessAsync(Guid userId)
    {
        if (GetCurrentUserId() == userId) return OperationResult.Success();

        var targetUserPriority = await GetUserHighestPriorityAsync(userId);
        if (!targetUserPriority.IsSuccess) return targetUserPriority.ToOperationResult();

        var currentUserPriority = GetCurrentUserHighestPriority();
        if (currentUserPriority > targetUserPriority)
        {
            return OperationResult.Forbidden(
                "You don't have sufficient permissions to manage this user. " +
                "Only users with higher or equal role priority can manage other users."
            );
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ValidateUserAccessAsync(string userName)
    {
        if (GetCurrentUserName() == userName) return OperationResult.Success();

        var userResult = await _userRepository.GetByUsernameAsync(userName);
        if (!userResult.IsSuccess) return userResult.ToOperationResult();

        return await ValidateUserAccessAsync(userResult!);
    }

    public Task<OperationResult> ValidateUserAccessAsync(User user) => ValidateUserAccessAsync(user.Id);

    public async Task<OperationResult> ValidateUsersAccessAsync(List<Guid> userIds)
    {
        foreach (var userId in userIds)
        {
            var result = await ValidateUserAccessAsync(userId);
            if (!result.IsSuccess) return result;
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ValidateUsersAccessAsync(List<string> userNames)
    {
        foreach (var userName in userNames)
        {
            var result = await ValidateUserAccessAsync(userName);
            if (!result.IsSuccess) return result;
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ValidateUsersAccessAsync(List<User> users)
    {
        foreach (var user in users)
        {
            var result = await ValidateUserAccessAsync(user);
            if (!result.IsSuccess) return result;
        }

        return OperationResult.Success();
    }

    #endregion

    #region Group Validation

    public async Task<OperationResult> ValidateGroupAccessAsync(Guid groupId)
    {
        if (GetCurrentUserGroupsIds().Contains(groupId)) return OperationResult.Success();

        var groupResult = await ExecuteAsync(() =>
            _groupRepository.AsQueryable()
                .Include(g => g.GroupRoles)
                .ThenInclude(gr => gr.Role)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == groupId)
        );

        if (!groupResult.IsSuccess) return groupResult.ToOperationResult();
        if (groupResult.Data == null) return OperationResult.NotFound($"Group with ID {groupId} not found");

        // Check if group has roles with higher priority than current user
        var groupRolesPriority = GetRolesPriority(groupResult.Data.GroupRoles.Select(gr => gr.Role));
        var currentUserPriority = GetCurrentUserHighestPriority();

        if (currentUserPriority > groupRolesPriority)
        {
            return OperationResult.Forbidden(
                "You don't have sufficient permissions to manage this group. " +
                "This group contains roles with higher priority than yours.");
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ValidateGroupAccessAsync(string groupName)
    {
        if (GetCurrentUserGroupsNames().Contains(groupName)) return OperationResult.Success();

        var groupIdResult = await ExecuteAsync(() =>
            _groupRepository.AsQueryable()
                .Where(g => g.Name == groupName)
                .Select(g => g.Id)
                .FirstOrDefaultAsync()
        );

        if (!groupIdResult.IsSuccess) return groupIdResult.ToOperationResult();
        if (groupIdResult.Data == Guid.Empty) return OperationResult.NotFound($"Group with name '{groupName}' not found");

        return await ValidateGroupAccessAsync(groupIdResult);
    }

    public Task<OperationResult> ValidateGroupAccessAsync(Group group) => ValidateGroupAccessAsync(group.Id);

    public async Task<OperationResult> ValidateGroupsAccessAsync(List<Guid> groupIds)
    {
        foreach (var groupId in groupIds)
        {
            var result = await ValidateGroupAccessAsync(groupId);
            if (!result.IsSuccess) return result;
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ValidateGroupsAccessAsync(List<string> groupNames)
    {
        foreach (var groupName in groupNames)
        {
            var result = await ValidateGroupAccessAsync(groupName);
            if (!result.IsSuccess) return result;
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ValidateGroupsAccessAsync(List<Group> groups)
    {
        foreach (var group in groups)
        {
            var result = await ValidateGroupAccessAsync(group);
            if (!result.IsSuccess) return result;
        }

        return OperationResult.Success();
    }

    #endregion

    #region Role Priority

    private int GetRolePriority(string roleName) => RolePriority.GetValueOrDefault(roleName, int.MaxValue);

    private int GetCurrentUserHighestPriority()
    {
        var roleNames = User.Claims
            .Where(c => c.Type == Constants.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return GetRolesPriority(roleNames);
    }

    private Task<OperationResult<int>> GetUserHighestPriorityAsync(Guid userId)
    {
        return ExecuteAsync(async Task<int> () =>
        {
            var user = await _userRepository.AsQueryable()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
                .ThenInclude(g => g.GroupRoles)
                .ThenInclude(gr => gr.Role)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return OperationResult<int>.BadRequest($"User with ID {userId} not found");

            var directRoles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .Where(n => n != null);

            var groupRoles = user.UserGroups
                .SelectMany(ug => ug.Group.GroupRoles.Select(gr => gr.Role.Name))
                .Where(n => n != null);

            var allRoles = directRoles
                .Concat(groupRoles)
                .Distinct()!
                .ToList<string>();

            return GetRolesPriority(allRoles);
        });
    }

    private int GetRolesPriority(IEnumerable<string> roleNames)
        => roleNames.Any()
            ? roleNames.Min(GetRolePriority)
            : int.MaxValue;

    private int GetRolesPriority(IEnumerable<Role> roles)
        => GetRolesPriority(roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
        );

    #endregion
}