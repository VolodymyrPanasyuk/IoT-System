namespace IoT_System.Application.DTOs.Response.Roles;

public class RoleShortResponse : BaseNamedEntityResponse
{
    public bool IsInherited { get; set; }
    public bool IsSystemDefault { get; set; }
}