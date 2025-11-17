using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;

namespace IoT_System.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<OperationResult<RefreshToken?>> GetByTokenAsync(string token);
    Task<OperationResult> DeleteByUserIdAsync(Guid userId);
}