using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task DeleteByUserIdAsync(Guid userId);
}