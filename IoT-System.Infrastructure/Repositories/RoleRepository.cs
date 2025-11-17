using static IoT_System.Application.Common.Helpers.ExecutionHelper;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

public class RoleRepository(AuthDbContext context) : RepositoryBase<Role>(context), IRoleRepository
{
    public Task<OperationResult<Role?>> GetByNameAsync(string roleName)
    {
        return ExecuteAsync(() => _dbSet.FirstOrDefaultAsync(r => r.Name == roleName));
    }
}