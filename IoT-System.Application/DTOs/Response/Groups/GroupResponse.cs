using IoT_System.Application.DTOs.Response.Roles;
using IoT_System.Application.DTOs.Response.Users;

namespace IoT_System.Application.DTOs.Response.Groups;

public class GroupResponse : BaseNamedEntityResponse
{
    public List<UserShortResponse> Users { get; set; } = new();
    public List<RoleShortResponse> Roles { get; set; } = new();
}