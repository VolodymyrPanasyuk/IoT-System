using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Contexts;

public class AuthDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
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
        // Set default schema
        builder.HasDefaultSchema("IoT.Identity");

        // Call base method first
        base.OnModelCreating(builder);

        // Rename standard Identity tables
        builder.Entity<User>(b => b.ToTable("Users"));
        builder.Entity<Role>(b => b.ToTable("Roles"));
        builder.Entity<UserRole>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));
        builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));

        // Configure custom tables
        builder.Entity<UserGroup>().HasKey(ug => new { ug.UserId, ug.GroupId });
        builder.Entity<GroupRole>().HasKey(gr => new { gr.GroupId, gr.RoleId });
        builder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

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

        builder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        builder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);
    }
}