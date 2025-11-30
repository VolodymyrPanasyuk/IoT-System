using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.Auth;

public class UserRepository(AuthDbContext context) : RepositoryBase<User, AuthDbContext>(context), IUserRepository
{
    public Task<OperationResult<User?>> GetByUsernameAsync(string username)
    {
        return ExecuteAsync(() =>
        {
            return _dbSet
                .Include(u => u.UserGroups)
                .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync(u => u.UserName == username);
        });
    }
}