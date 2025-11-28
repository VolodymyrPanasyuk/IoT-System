using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.DTOs.Response.Roles;

namespace IoT_System.Application.DTOs.Response.Users;

public class UserResponse : UserShortResponse
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public List<RoleShortResponse> Roles { get; set; } = new();
    public List<GroupShortResponse> Groups { get; set; } = new();
}