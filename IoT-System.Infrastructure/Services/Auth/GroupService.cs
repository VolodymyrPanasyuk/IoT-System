using System.Net;
using AutoMapper;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Services.Auth;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IGroupRoleRepository _groupRoleRepository;
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IMapper _mapper;

    public GroupService(
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IGroupRoleRepository groupRoleRepository,
        IUserGroupRepository userGroupRepository,
        IUserRoleRepository userRoleRepository,
        IMapper mapper)
    {
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _groupRoleRepository = groupRoleRepository;
        _userGroupRepository = userGroupRepository;
        _userRoleRepository = userRoleRepository;
        _mapper = mapper;
    }

    public async Task<OperationResult<GroupResponse>> CreateGroupAsync(CreateGroupRequest request)
    {
        var existingGroup = await _groupRepository.AsQueryable().FirstOrDefaultAsync(g => g.Name == request.Name);
        if (existingGroup != null)
        {
            return OperationResult<GroupResponse>.Conflict($"Group with name '{request.Name}' already exists");
        }

        var errors = new List<string>();
        var group = _mapper.Map<Group>(request);

        var createGroupResult = await _groupRepository.AddAsync(group);
        if (!createGroupResult.IsSuccess || createGroupResult.Data == null)
        {
            errors.AddRange(createGroupResult.Errors);
            return OperationResult<GroupResponse>.Failure(createGroupResult.Exception, errors);
        }

        if (request.UserIds is { Count: > 0 })
        {
            var usersToAdd = await _userRepository.AsQueryable()
                .Where(u => request.UserIds.Contains(u.Id))
                .ToListAsync();

            if (usersToAdd.Count != request.UserIds.Count) errors.Add("One or more users do not exist");

            var addUsersResult = await _userGroupRepository.AddRangeAsync(usersToAdd.Select(u => new UserGroup { UserId = u.Id, GroupId = group.Id }));
            if (!addUsersResult.IsSuccess) errors.AddRange(addUsersResult.Errors);
        }

        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var rolesToAdd = await _roleRepository.AsQueryable()
                .Where(r => request.RoleIds.Contains(r.Id) && r.Name != null)
                .ToListAsync();

            if (rolesToAdd.Count != request.RoleIds.Count) errors.Add("One or more roles do not exist");

            var addRolesResult = await _groupRoleRepository.AddRangeAsync(rolesToAdd.Select(r => new GroupRole { GroupId = group.Id, RoleId = r.Id }));
            if (!addRolesResult.IsSuccess) errors.AddRange(addRolesResult.Errors);
        }

        var result = await GetByIdAsync(createGroupResult.Data.Id);
        if (errors.Any()) result.Errors = result.Errors.Concat(errors);
        if (result.IsSuccess) result.StatusCode = HttpStatusCode.Created;
        return result;
    }

    public async Task<OperationResult<GroupResponse>> UpdateGroupAsync(UpdateGroupRequest request)
    {
        var groupResult = await _groupRepository.GetByIdAsync(request.Id);
        if (!groupResult.IsSuccess || groupResult.Data == null)
        {
            return OperationResult<GroupResponse>.NotFound($"Group with ID {request.Id} not found");
        }

        var errors = new List<string>();
        var group = groupResult.Data;
        group.Name = request.Name;

        var updateGroupResult = await _groupRepository.UpdateAsync(group);
        if (!updateGroupResult.IsSuccess)
        {
            errors.AddRange(updateGroupResult.Errors);
            return OperationResult<GroupResponse>.Failure(updateGroupResult.Exception, errors);
        }

        if (request.AddUsers is { Count: > 0 })
        {
            var usersToAdd = await _userRepository.AsQueryable()
                .Where(u => request.AddUsers.Contains(u.Id))
                .ToListAsync();

            if (usersToAdd.Count != request.AddUsers.Count) errors.Add("One or more users do not exist");

            var addUsersResult = await _userGroupRepository.AddRangeAsync(usersToAdd.Select(u => new UserGroup { UserId = u.Id, GroupId = group.Id }));
            if (!addUsersResult.IsSuccess) errors.AddRange(addUsersResult.Errors);
        }

        if (request.RemoveUsers is { Count: > 0 })
        {
            var usersToRemove = await _userGroupRepository.AsQueryable()
                .Where(ug => ug.GroupId == group.Id && request.RemoveUsers.Contains(ug.UserId))
                .ToListAsync();

            if (usersToRemove.Count != request.RemoveUsers.Count) errors.Add("One or more users do not exist");

            var removeUsersResult = await _userGroupRepository.DeleteRangeAsync(usersToRemove);
            if (!removeUsersResult.IsSuccess) errors.AddRange(removeUsersResult.Errors);
        }

        if (request.AddRoles is { Count: > 0 })
        {
            var rolesToAdd = await _roleRepository.AsQueryable()
                .Where(r => request.AddRoles.Contains(r.Id) && r.Name != null)
                .ToListAsync();

            if (rolesToAdd.Count != request.AddRoles.Count) errors.Add("One or more roles do not exist");

            var addRolesResult = await _groupRoleRepository.AddRangeAsync(rolesToAdd.Select(r => new GroupRole { GroupId = group.Id, RoleId = r.Id }));
            if (!addRolesResult.IsSuccess) errors.AddRange(addRolesResult.Errors);
        }

        if (request.RemoveRoles is { Count: > 0 })
        {
            var rolesToRemove = await _groupRoleRepository.AsQueryable()
                .Where(gr => gr.GroupId == group.Id && request.RemoveRoles.Contains(gr.RoleId))
                .ToListAsync();

            if (rolesToRemove.Count != request.RemoveRoles.Count) errors.Add("One or more roles do not exist");

            var removeRolesResult = await _groupRoleRepository.DeleteRangeAsync(rolesToRemove);
            if (!removeRolesResult.IsSuccess) errors.AddRange(removeRolesResult.Errors);
        }

        var result = await GetByIdAsync(group.Id);
        if (errors.Any()) result.Errors = result.Errors.Concat(errors);
        return result;
    }

    public async Task<OperationResult> DeleteGroupAsync(Guid id)
    {
        var groupResult = await _groupRepository.GetByIdAsync(id);
        if (!groupResult.IsSuccess || groupResult.Data == null)
        {
            return OperationResult.NotFound($"Group with ID {id} not found");
        }

        var deleteResult = await _groupRepository.DeleteAsync(groupResult.Data);
        if (!deleteResult.IsSuccess)
        {
            return OperationResult.Failure(deleteResult.Exception, deleteResult.Errors);
        }

        return OperationResult.NoContent();
    }

    public async Task<OperationResult<GroupResponse>> GetByIdAsync(Guid id, bool includeUsersAndRoles = false)
    {
        var group = await GetGroupsQuery(includeUsersAndRoles).FirstOrDefaultAsync(g => g.Id == id);
        if (group == null)
        {
            return OperationResult<GroupResponse>.NotFound($"Group with ID {id} not found");
        }

        var response = _mapper.Map<GroupResponse>(group);
        return OperationResult<GroupResponse>.Success(response);
    }

    public async Task<OperationResult<IEnumerable<GroupResponse>>> GetAllAsync(bool includeUsersAndRoles = false)
    {
        var groups = await GetGroupsQuery(includeUsersAndRoles).ToListAsync();
        var response = _mapper.Map<IEnumerable<GroupResponse>>(groups);
        return OperationResult<IEnumerable<GroupResponse>>.Success(response);
    }

    #region Private Methods

    private IQueryable<Group> GetGroupsQuery(bool includeUsersAndRoles = false)
    {
        var query = _groupRepository.AsQueryable();
        if (includeUsersAndRoles)
        {
            query = query
                .Include(g => g.GroupRoles)
                .ThenInclude(gr => gr.Role)
                .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                .AsSplitQuery();
        }

        return query;
    }

    #endregion
}