using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Interfaces.Services;

public interface IAccessValidationService
{
    // ====== Current User Information ======
    Guid GetCurrentUserId();
    string GetCurrentUserName();
    List<Guid> GetCurrentUserRolesIds();
    List<string> GetCurrentUserRolesNames();
    Task<OperationResult<List<Role>>> GetCurrentUserRoles();
    List<Guid> GetCurrentUserGroupsIds();
    List<string> GetCurrentUserGroupsNames();
    Task<OperationResult<List<Group>>> GetCurrentUserGroups();
    Task<OperationResult<User>> GetCurrentUserAsync(bool includeRolesAndGroups = false);

    // ====== Role Validation ======
    Task<OperationResult> ValidateRolesAccessAsync(List<Guid> roleIds);
    Task<OperationResult> ValidateRolesAccessAsync(List<string> roleNames);
    Task<OperationResult> ValidateRolesAccessAsync(List<Role> roles);

    // ====== User Validation ======
    Task<OperationResult> ValidateUserAccessAsync(Guid userId);
    Task<OperationResult> ValidateUserAccessAsync(string userName);
    Task<OperationResult> ValidateUserAccessAsync(User user);
    Task<OperationResult> ValidateUsersAccessAsync(List<Guid> userIds);
    Task<OperationResult> ValidateUsersAccessAsync(List<string> userNames);
    Task<OperationResult> ValidateUsersAccessAsync(List<User> users);

    // ====== Group Validation ======
    Task<OperationResult> ValidateGroupAccessAsync(Guid groupId);
    Task<OperationResult> ValidateGroupAccessAsync(string groupName);
    Task<OperationResult> ValidateGroupAccessAsync(Group group);
    Task<OperationResult> ValidateGroupsAccessAsync(List<Guid> groupIds);
    Task<OperationResult> ValidateGroupsAccessAsync(List<string> groupNames);
    Task<OperationResult> ValidateGroupsAccessAsync(List<Group> groups);
}