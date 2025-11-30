using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.Auth;

public class UserRoleRepository(AuthDbContext context) : RepositoryBase<UserRole, AuthDbContext>(context), IUserRoleRepository
{
    public override Task<OperationResult<UserRole?>> GetByIdAsync(Guid id)
        => ExecuteAsync(async () => await _dbSet.FirstOrDefaultAsync(ur => ur.UserId == id || ur.RoleId == id));
}