using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.DTOs.Response.Users;

namespace IoT_System.Application.DTOs.Response.Roles;

public class RoleResponse : BaseNamedEntityResponse
{
    public bool IsSystemDefault { get; set; }
    public List<UserShortResponse> Users { get; set; } = new();
    public List<GroupShortResponse> Groups { get; set; } = new();
}