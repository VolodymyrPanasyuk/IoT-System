using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Contexts;

public class AuthDbContext : IdentityDbContext<User, Role, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<Group> Groups => Set<Group>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<GroupRole> GroupRoles => Set<GroupRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserGroup>().HasKey(ug => new { ug.UserId, ug.GroupId });
        builder.Entity<GroupRole>().HasKey(gr => new { gr.GroupId, gr.RoleId });

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        builder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        builder.Entity<GroupRole>()
            .HasOne(gr => gr.Group)
            .WithMany(g => g.GroupRoles)
            .HasForeignKey(gr => gr.GroupId);

        builder.Entity<GroupRole>()
            .HasOne(gr => gr.Role)
            .WithMany(r => r.GroupRoles)
            .HasForeignKey(gr => gr.RoleId);
    }
}