using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.Auth;

public class RefreshTokenRepository(AuthDbContext context) : RepositoryBase<RefreshToken, AuthDbContext>(context), IRefreshTokenRepository
{
    public Task<OperationResult<RefreshToken?>> GetByTokenAsync(string token)
    {
        return ExecuteAsync(() => _dbSet.FirstOrDefaultAsync(rt => rt.Token == token));
    }

    public Task<OperationResult> DeleteByUserIdAsync(Guid userId)
    {
        return ExecuteAsync(async () =>
        {
            var tokens = _dbSet.Where(rt => rt.UserId == userId);
            _dbSet.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        });
    }
}