using IoT_System.Application.Common;
using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoT_System.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await SeedRolesAsync(roleManager);
        await SeedSuperAdminAsync(userManager, configuration);
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
    {
        foreach (var roleName in Constants.Roles.SystemDefault)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role { Name = roleName });
            }
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<User> userManager, IConfiguration configuration)
    {
        var superAdminPassword = configuration["DefaultSuperAdmin:Password"];
        if (string.IsNullOrEmpty(superAdminPassword))
        {
            throw new InvalidOperationException(
                "DefaultSuperAdmin:Password is not configured. Please set it in your .env or appsettings.json file.");
        }

        var isAnySuperAdmin = await userManager.Users.AnyAsync(u => u.UserRoles.Any(ur => ur.Role.Name == Constants.Roles.SuperAdmin));
        if (!isAnySuperAdmin)
        {
            var superAdmin = new User
            {
                UserName = Constants.DefaultSuperAdmin.UserName,
                Email = Constants.DefaultSuperAdmin.Email,
                FirstName = Constants.DefaultSuperAdmin.FirstName,
                LastName = Constants.DefaultSuperAdmin.LastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, Constants.Roles.SuperAdmin);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create SuperAdmin user: {errors}");
            }
        }
    }
}