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

    public Task<OperationResult> DeleteAllByUserIdAsync(Guid userId)
    {
        return ExecuteAsync(async () =>
        {
            var refreshTokens = await _dbSet
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            _dbSet.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();
        });
    }

    public Task<OperationResult> DeleteAllExpiredByUserIdAsync(Guid userId)
    {
        return ExecuteAsync(async () =>
        {
            var refreshTokens = await _dbSet
                .Where(rt => rt.UserId == userId && rt.ExpiryDate < DateTime.UtcNow)
                .ToListAsync();

            _dbSet.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();
        });
    }

    public Task<OperationResult> DeleteByTokenAsync(string token)
    {
        return ExecuteAsync(async () =>
        {
            var refreshToken = await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken == null) return;

            _dbSet.Remove(refreshToken);
            await _context.SaveChangesAsync();
        });
    }
}