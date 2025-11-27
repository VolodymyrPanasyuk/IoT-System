using System.Net;
using AutoMapper;
using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Users;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IGroupRoleRepository _groupRoleRepository;
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IMapper _mapper;

    public UserService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        IRoleRepository roleRepository,
        IGroupRoleRepository groupRoleRepository,
        IUserGroupRepository userGroupRepository,
        IUserRoleRepository userRoleRepository,
        IMapper mapper)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _roleRepository = roleRepository;
        _groupRoleRepository = groupRoleRepository;
        _userGroupRepository = userGroupRepository;
        _userRoleRepository = userRoleRepository;
        _mapper = mapper;
    }

    public async Task<OperationResult<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
        {
            return OperationResult<UserResponse>.Conflict("User with this username already exists");
        }

        var errors = new List<string>();
        var user = _mapper.Map<User>(request);

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            errors.AddRange(createUserResult.Errors.Select(e => e.Description));
            return OperationResult<UserResponse>.BadRequest(errors);
        }

        if (request.RoleIds is { Count: > 0 })
        {
            var roles = await _roleRepository.AsQueryable()
                .Where(r => request.RoleIds.Contains(r.Id) && r.Name != null)
                .ToListAsync();

            if (roles.Count != request.RoleIds.Count) errors.Add("One or more roles do not exist");

            var addRolesResult = await _userManager.AddToRolesAsync(user, roles.Select(r => r.Name!));
            if (!addRolesResult.Succeeded) errors.AddRange(addRolesResult.Errors.Select(e => e.Description));
        }

        var result = await GetByIdAsync(user.Id, true);
        if (errors.Any()) result.Errors = result.Errors.Concat(errors);
        if (result.IsSuccess) result.StatusCode = HttpStatusCode.Created;
        return result;
    }

    public async Task<OperationResult<UserResponse>> UpdateUserAsync(UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
        {
            return OperationResult<UserResponse>.NotFound($"User with ID {request.Id} not found");
        }

        var errors = new List<string>();
        user.UserName = request.UserName;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        var updateUserResult = await _userManager.UpdateAsync(user);
        if (!updateUserResult.Succeeded)
        {
            errors.AddRange(updateUserResult.Errors.Select(e => e.Description));
            return OperationResult<UserResponse>.BadRequest(errors);
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);

            if (!passwordResult.Succeeded)
            {
                errors.AddRange(passwordResult.Errors.Select(e => e.Description));
                return OperationResult<UserResponse>.BadRequest(errors);
            }
        }

        if (request.RolesToAssign is { Count: > 0 })
        {
            var rolesToAssign = await _roleRepository.AsQueryable()
                .Where(r => request.RolesToAssign.Contains(r.Id) && r.Name != null)
                .ToListAsync();

            if (rolesToAssign.Count != request.RolesToAssign.Count) errors.Add("One or more roles do not exist");

            var assignRolesResult = await _userManager.AddToRolesAsync(user, rolesToAssign.Select(r => r.Name!));
            if (!assignRolesResult.Succeeded) errors.AddRange(assignRolesResult.Errors.Select(e => e.Description));
        }

        if (request.RolesToRemove is { Count: > 0 })
        {
            var rolesToRemove = await _roleRepository.AsQueryable()
                .Where(r => request.RolesToRemove.Contains(r.Id) && r.Name != null)
                .ToListAsync();

            if (rolesToRemove.Count != request.RolesToRemove.Count) errors.Add("One or more roles do not exist");

            var superAdminRole = rolesToRemove.FirstOrDefault(r => r.Name == Constants.Roles.SuperAdmin);
            if (superAdminRole != null)
            {
                var isAnyOtherSuperAdmin = await _userManager.Users.AnyAsync(u => u.Id != user.Id && u.UserRoles.Any(ur => ur.RoleId == superAdminRole.Id));
                if (!isAnyOtherSuperAdmin)
                {
                    rolesToRemove.Remove(superAdminRole);
                    errors.Add(
                        $"Cannot remove {Constants.Roles.SuperAdmin} role from the last {Constants.Roles.SuperAdmin} user. " +
                        $"At least one {Constants.Roles.SuperAdmin} must exist!");
                }
            }

            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove.Select(r => r.Name!));
            if (!removeRolesResult.Succeeded) errors.AddRange(removeRolesResult.Errors.Select(e => e.Description));
        }

        if (request.GroupsToAssign is { Count: > 0 })
        {
            var groupsToAssign = await _groupRepository.AsQueryable()
                .Where(g => request.GroupsToAssign.Contains(g.Id))
                .ToListAsync();

            if (groupsToAssign.Count != request.GroupsToAssign.Count) errors.Add("One or more groups do not exist");

            var assignGroupsResult = await _userGroupRepository.AddRangeAsync(groupsToAssign.Select(g => new UserGroup { UserId = user.Id, GroupId = g.Id }));
            if (!assignGroupsResult.IsSuccess) errors.AddRange(assignGroupsResult.Errors);
        }

        if (request.GroupsToRemove is { Count: > 0 })
        {
            var groupsToRemove = await _userGroupRepository.AsQueryable()
                .Where(ug => ug.UserId == user.Id && request.GroupsToRemove.Contains(ug.GroupId))
                .ToListAsync();

            if (groupsToRemove.Count != request.GroupsToRemove.Count) errors.Add("One or more groups do not exist");

            var removeGroupsResult = await _userGroupRepository.DeleteRangeAsync(groupsToRemove);
            if (!removeGroupsResult.IsSuccess) errors.AddRange(removeGroupsResult.Errors);
        }

        var result = await GetByIdAsync(user.Id, true);
        if (errors.Any()) result.Errors = result.Errors.Concat(errors);
        return result;
    }

    public async Task<OperationResult> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return OperationResult.NotFound($"User with ID {userId} not found");
        }

        var superAdminRole = user.UserRoles
            .Select(ur => ur.Role)
            .FirstOrDefault(r => r.Name == Constants.Roles.SuperAdmin);

        if (superAdminRole != null)
        {
            var isAnyOtherSuperAdmin = await _userManager.Users.AnyAsync(u => u.Id != userId && u.UserRoles.Any(ur => ur.RoleId == superAdminRole.Id));
            if (!isAnyOtherSuperAdmin)
            {
                return OperationResult.BadRequest($"Cannot delete the last {Constants.Roles.SuperAdmin} user. " +
                                                  $"At least one {Constants.Roles.SuperAdmin} must exist in the system!");
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return OperationResult.BadRequest(errors);
        }

        return OperationResult.NoContent();
    }

    public async Task<OperationResult<UserResponse>> GetByIdAsync(Guid id, bool includeGroupsAndRoles = false)
    {
        var user = await GetUsersQuery(includeGroupsAndRoles).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return OperationResult<UserResponse>.NotFound($"User with ID {id} not found");
        }

        var response = _mapper.Map<UserResponse>(user);
        return OperationResult<UserResponse>.Success(response);
    }

    public async Task<OperationResult<UserResponse>> GetByUsernameAsync(string username, bool includeGroupsAndRoles = false)
    {
        var userQuery = GetUsersQuery(includeGroupsAndRoles).Where(u => u.UserName == username);
        if (!await userQuery.AnyAsync())
        {
            return OperationResult<UserResponse>.NotFound($"User with username '{username}' not found");
        }

        var user = await userQuery.FirstOrDefaultAsync();
        var response = _mapper.Map<UserResponse>(user);
        return OperationResult<UserResponse>.Success(response);
    }

    public async Task<OperationResult<IEnumerable<UserResponse>>> GetAllAsync(bool includeGroupsAndRoles = false)
    {
        var users = await GetUsersQuery(includeGroupsAndRoles).ToListAsync();
        var response = _mapper.Map<IEnumerable<UserResponse>>(users);
        return OperationResult<IEnumerable<UserResponse>>.Success(response);
    }

    #region Private Methods

    private IQueryable<User> GetUsersQuery(bool includeGroupsAndRoles = false)
    {
        var query = _userManager.Users;
        if (includeGroupsAndRoles)
        {
            query = query
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
                .ThenInclude(g => g.GroupRoles)
                .ThenInclude(gr => gr.Role)
                .AsSplitQuery();
        }

        return query;
    }

    #endregion
}