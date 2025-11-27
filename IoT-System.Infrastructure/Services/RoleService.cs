using System.Net;
using AutoMapper;
using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Roles;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupRoleRepository _groupRoleRepository;
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IMapper _mapper;

    public RoleService(
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IGroupRepository groupRepository,
        IGroupRoleRepository groupRoleRepository,
        IUserGroupRepository userGroupRepository,
        IUserRoleRepository userRoleRepository,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _groupRepository = groupRepository;
        _groupRoleRepository = groupRoleRepository;
        _userGroupRepository = userGroupRepository;
        _userRoleRepository = userRoleRepository;
        _mapper = mapper;
    }

    public async Task<OperationResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest request)
    {
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null)
        {
            return OperationResult<RoleResponse>.Conflict($"Role with name '{request.Name}' already exists");
        }

        var errors = new List<string>();
        var role = _mapper.Map<Role>(request);

        var createRoleResult = await _roleManager.CreateAsync(role);
        if (!createRoleResult.Succeeded)
        {
            errors.AddRange(createRoleResult.Errors.Select(e => e.Description));
            return OperationResult<RoleResponse>.BadRequest(errors);
        }

        var result = await GetByIdAsync(role.Id);
        if (errors.Any()) result.Errors = result.Errors.Concat(errors);
        if (result.IsSuccess) result.StatusCode = HttpStatusCode.Created;
        return result;
    }

    public async Task<OperationResult<RoleResponse>> UpdateRoleAsync(UpdateRoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null)
        {
            return OperationResult<RoleResponse>.NotFound($"Role with ID {request.Id} not found");
        }

        if (Constants.Roles.SystemDefault.Contains(role.Name ?? string.Empty) && request.Name != role.Name)
        {
            return OperationResult<RoleResponse>.BadRequest($"Role '{role.Name}' cannot be modified because it is a system default role!");
        }

        var errors = new List<string>();
        if (request.Name != role.Name)
        {
            role.Name = request.Name;

            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                errors.AddRange(updateResult.Errors.Select(e => e.Description));
                return OperationResult<RoleResponse>.BadRequest(errors);
            }
        }

        if (request.AddToUsers is { Count: > 0 })
        {
            var usersToAdd = await _userRepository.AsQueryable()
                .Where(u => request.AddToUsers.Contains(u.Id))
                .ToListAsync();

            if (usersToAdd.Count != request.AddToUsers.Count) errors.Add("One or more users do not exist");

            foreach (var userToAdd in usersToAdd)
            {
                var addUserResult = await _userManager.AddToRoleAsync(userToAdd, role.Name!);
                if (!addUserResult.Succeeded) errors.AddRange(addUserResult.Errors.Select(e => e.Description));
            }
        }

        if (request.RemoveFromUsers is { Count: > 0 })
        {
            var usersToRemove = await _userRepository.AsQueryable()
                .Where(u => request.RemoveFromUsers.Contains(u.Id))
                .ToListAsync();

            if (usersToRemove.Count != request.RemoveFromUsers.Count) errors.Add("One or more users do not exist");

            if (role.Name == Constants.Roles.SuperAdmin)
            {
                var isAnyOtherSuperAdmins = await _userRepository.AsQueryable()
                    .Where(u => !usersToRemove.Select(x => x.Id).Contains(u.Id)
                                && u.UserRoles.Any(ur => ur.RoleId == role.Id)
                    ).AnyAsync();

                if (!isAnyOtherSuperAdmins)
                {
                    errors.Add($"Cannot remove {Constants.Roles.SuperAdmin} role from all {Constants.Roles.SuperAdmin} users. " +
                               $"At least one {Constants.Roles.SuperAdmin} must exist in the system!");

                    return OperationResult<RoleResponse>.BadRequest(errors);
                }
            }

            foreach (var userToRemove in usersToRemove)
            {
                var removeUserResult = await _userManager.RemoveFromRoleAsync(userToRemove, role.Name!);
                if (!removeUserResult.Succeeded) errors.AddRange(removeUserResult.Errors.Select(e => e.Description));
            }
        }

        if (request.AddToGroups is { Count: > 0 })
        {
            var groupsToAdd = await _groupRepository.AsQueryable()
                .Where(g => request.AddToGroups.Contains(g.Id))
                .ToListAsync();

            if (groupsToAdd.Count != request.AddToGroups.Count) errors.Add("One or more groups do not exist");

            var addGroupsResult = await _groupRoleRepository.AddRangeAsync(groupsToAdd.Select(g => new GroupRole { GroupId = g.Id, RoleId = role.Id }));
            if (!addGroupsResult.IsSuccess) errors.AddRange(addGroupsResult.Errors);
        }

        if (request.RemoveFromGroups is { Count: > 0 })
        {
            var groupsToRemove = await _groupRoleRepository.AsQueryable()
                .Where(gr => request.RemoveFromGroups.Contains(gr.GroupId) && gr.RoleId == role.Id)
                .ToListAsync();

            if (groupsToRemove.Count != request.RemoveFromGroups.Count) errors.Add("One or more groups do not exist");

            var removeGroupsResult = await _groupRoleRepository.DeleteRangeAsync(groupsToRemove);
            if (!removeGroupsResult.IsSuccess) errors.AddRange(removeGroupsResult.Errors);
        }

        var result = await GetByIdAsync(role.Id);
        if (errors.Any()) result.Errors = result.Errors.Concat(errors);
        return result;
    }

    public async Task<OperationResult> DeleteRoleAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
        {
            return OperationResult.NotFound($"Role with ID {id} not found");
        }

        if (Constants.Roles.SystemDefault.Contains(role.Name ?? string.Empty))
        {
            return OperationResult<RoleResponse>.BadRequest($"Role '{role.Name}' cannot be deleted because it is a system default role!");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return OperationResult.BadRequest(errors);
        }

        return OperationResult.NoContent();
    }

    public async Task<OperationResult<RoleResponse>> GetByIdAsync(Guid id, bool includeUsersAndGroups = false)
    {
        var role = await GetRolesQuery(includeUsersAndGroups).FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
        {
            return OperationResult<RoleResponse>.NotFound($"Role with ID {id} not found");
        }

        var response = _mapper.Map<RoleResponse>(role);
        return OperationResult<RoleResponse>.Success(response);
    }

    public async Task<OperationResult<RoleResponse>> GetByNameAsync(string name, bool includeUsersAndGroups = false)
    {
        var rolesQuery = GetRolesQuery(includeUsersAndGroups).Where(r => r.Name == name);
        if (!await rolesQuery.AnyAsync())
        {
            return OperationResult<RoleResponse>.NotFound($"Role with name '{name}' not found");
        }

        var role = await rolesQuery.FirstOrDefaultAsync();
        var response = _mapper.Map<RoleResponse>(role);
        return OperationResult<RoleResponse>.Success(response);
    }

    public async Task<OperationResult<IEnumerable<RoleResponse>>> GetAllAsync(bool includeUsersAndGroups = false)
    {
        var roles = await GetRolesQuery(includeUsersAndGroups).ToListAsync();
        var response = _mapper.Map<IEnumerable<RoleResponse>>(roles);
        return OperationResult<IEnumerable<RoleResponse>>.Success(response);
    }

    #region Private Methods

    private IQueryable<Role> GetRolesQuery(bool includeUsersAndGroups = false)
    {
        var query = _roleRepository.AsQueryable();
        if (includeUsersAndGroups)
        {
            query = query
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
                .Include(r => r.GroupRoles)
                .ThenInclude(gr => gr.Group)
                .AsSplitQuery();
        }

        return query;
    }

    #endregion
}