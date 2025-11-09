using Microsoft.AspNetCore.Identity;

namespace IoT_System.Domain.Entities.Auth;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}