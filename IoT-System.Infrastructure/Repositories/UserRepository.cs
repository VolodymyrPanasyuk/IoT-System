using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

public class UserRepository(AuthDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.UserGroups)
            .ThenInclude(ug => ug.Group)
            .FirstOrDefaultAsync(u => u.UserName == username);
    }
}