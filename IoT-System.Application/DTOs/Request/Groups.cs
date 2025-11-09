namespace IoT_System.Application.DTOs.Request;

public record CreateGroupRequest(
    string Name,
    List<Guid>? UserIds = null,
    List<Guid>? RoleIds = null
) : BaseNamedEntityDto(Name);

public record UpdateGroupRequest(
    Guid Id,
    string Name,
    List<Guid>? AddUsers = null,
    List<Guid>? RemoveUsers = null,
    List<Guid>? AddRoles = null,
    List<Guid>? RemoveRoles = null
) : BaseNamedEntityDto(Name);

public record AssignUsersToGroupRequest(Guid GroupId, List<Guid> UserIds);

public record RemoveUsersFromGroupRequest(Guid GroupId, List<Guid> UserIds);

public record AssignRolesToGroupRequest(Guid GroupId, List<Guid> RoleIds);

public record RemoveRolesFromGroupRequest(Guid GroupId, List<Guid> RoleIds);