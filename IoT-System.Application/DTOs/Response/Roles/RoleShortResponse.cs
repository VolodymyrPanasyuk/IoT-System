namespace IoT_System.Application.DTOs.Response.Roles;

public class RoleShortResponse : BaseEntityResponse
{
    public string Name { get; set; } = null!;
    public bool IsInherited { get; set; }
    public bool IsSystemDefault { get; set; }
}