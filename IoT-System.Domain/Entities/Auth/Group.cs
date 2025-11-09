namespace IoT_System.Domain.Entities.Auth;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<GroupRole> GroupRoles { get; set; } = new List<GroupRole>();
}