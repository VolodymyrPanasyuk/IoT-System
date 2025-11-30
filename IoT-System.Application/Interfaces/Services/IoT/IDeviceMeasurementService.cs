using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IDeviceMeasurementService
{
    Task<OperationResult<DeviceMeasurement>> CreateAsync(DeviceMeasurement measurement);
    Task<OperationResult<DeviceMeasurement>> UpdateAsync(DeviceMeasurement measurement);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<DeviceMeasurement>> GetByIdAsync(Guid id);

    Task<OperationResult<List<DeviceMeasurement>>> GetByDeviceIdAsync(
        Guid deviceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null);

    Task<OperationResult<DeviceMeasurement>> GetLatestByDeviceIdAsync(Guid deviceId);
}