namespace IoT_System.Application.DTOs.Request;

public record CreateRoleRequest(string Name) : BaseNamedEntityDto(Name);

public record UpdateRoleRequest(
    Guid Id,
    string Name,
    List<Guid>? AddToUsers = null,
    List<Guid>? RemoveFromUsers = null,
    List<Guid>? AddToGroups = null,
    List<Guid>? RemoveFromGroups = null
) : BaseNamedEntityDto(Name);

public record AssignRoleToUsersRequest(Guid RoleId, List<Guid> UserIds);

public record RemoveRoleFromUsersRequest(Guid RoleId, List<Guid> UserIds);

public record AssignRoleToGroupsRequest(Guid RoleId, List<Guid> GroupIds);

public record RemoveRoleFromGroupsRequest(Guid RoleId, List<Guid> GroupIds);