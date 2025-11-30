using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IDeviceFieldRepository : IRepositoryBase<DeviceField>
{
    Task<OperationResult<List<DeviceField>>> GetByDeviceIdAsync(Guid deviceId, bool activeOnly = true);
    Task<OperationResult<bool>> IsFieldNameUniqueAsync(Guid deviceId, string fieldName, Guid? excludeFieldId = null);
}