using static IoT_System.Application.Common.Helpers.ExecutionHelper;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

public class UserGroupRepository(AuthDbContext context) : RepositoryBase<UserGroup, AuthDbContext>(context), IUserGroupRepository
{
    public override Task<OperationResult<UserGroup?>> GetByIdAsync(Guid id)
        => ExecuteAsync(async () => await _dbSet.FirstOrDefaultAsync(ug => ug.UserId == id || ug.GroupId == id));
}