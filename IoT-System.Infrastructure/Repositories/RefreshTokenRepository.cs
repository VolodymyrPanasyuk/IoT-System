using static IoT_System.Application.Common.Helpers.ExecutionHelper;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

public class RefreshTokenRepository(AuthDbContext context) : RepositoryBase<RefreshToken>(context), IRefreshTokenRepository
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