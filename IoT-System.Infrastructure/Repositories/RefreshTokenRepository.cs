using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Domain.Entities.Auth;
using IoT_System.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Repositories;

public class RefreshTokenRepository(AuthDbContext context) : RepositoryBase<RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task DeleteByUserIdAsync(Guid userId)
    {
        var tokens = _dbSet.Where(rt => rt.UserId == userId);
        _dbSet.RemoveRange(tokens);
        await _context.SaveChangesAsync();
    }
}