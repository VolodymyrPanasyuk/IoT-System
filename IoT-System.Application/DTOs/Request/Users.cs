namespace IoT_System.Application.DTOs.Request;

public record CreateUserRequest(
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    List<Guid>? RoleIds = null
) : BaseUserDto(UserName, Password, FirstName, LastName);

public record UpdateUserRequest(
    Guid Id,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    List<Guid>? RoleToAssign = null,
    List<Guid>? RoleToRemove = null,
    List<Guid>? GroupsToAssign = null,
    List<Guid>? GroupsToRemove = null
) : BaseUserDto(UserName, Password, FirstName, LastName);

public record AssignRolesToUserRequest(Guid UserId, List<Guid> RoleIds);

public record RemoveRolesFromUserRequest(Guid UserId, List<Guid> RoleIds);

public record AssignGroupsToUserRequest(Guid UserId, List<Guid> GroupIds);

public record RemoveGroupsFromUserRequest(Guid UserId, List<Guid> GroupIds);