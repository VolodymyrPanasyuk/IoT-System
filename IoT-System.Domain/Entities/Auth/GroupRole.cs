namespace IoT_System.Domain.Entities.Auth;

public class GroupRole
{
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}