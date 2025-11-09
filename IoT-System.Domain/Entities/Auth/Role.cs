using Microsoft.AspNetCore.Identity;

namespace IoT_System.Domain.Entities.Auth;

public class Role : IdentityRole<Guid>
{
    public ICollection<GroupRole> GroupRoles { get; set; } = new List<GroupRole>();
}