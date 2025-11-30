using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IDeviceRepository : IRepositoryBase<Device>
{
    Task<OperationResult<Device?>> GetByApiKeyAsync(string apiKey);
    Task<OperationResult<bool>> IsApiKeyUniqueAsync(string apiKey, Guid? excludeDeviceId = null);
}