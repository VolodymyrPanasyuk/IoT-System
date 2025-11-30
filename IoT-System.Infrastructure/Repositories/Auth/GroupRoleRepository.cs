using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.Auth;

public class GroupRoleRepository(AuthDbContext context) : RepositoryBase<GroupRole, AuthDbContext>(context), IGroupRoleRepository
{
    public override Task<OperationResult<GroupRole?>> GetByIdAsync(Guid id)
        => ExecuteAsync(async () => await _dbSet.FirstOrDefaultAsync(gr => gr.GroupId == id || gr.RoleId == id));
}