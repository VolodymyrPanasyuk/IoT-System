using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IDeviceMeasurementRepository : IRepositoryBase<DeviceMeasurement>
{
    Task<OperationResult<List<DeviceMeasurement>>> GetByDeviceIdAsync(
        Guid deviceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null);

    Task<OperationResult<DeviceMeasurement?>> GetLatestByDeviceIdAsync(Guid deviceId);
}