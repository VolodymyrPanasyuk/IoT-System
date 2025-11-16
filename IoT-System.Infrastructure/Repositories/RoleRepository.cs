using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

public class RoleRepository(AuthDbContext context) : RepositoryBase<Role>(context), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Name == roleName);
    }
}