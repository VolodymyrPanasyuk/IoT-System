using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IIoTHubService
{
    Task NotifyMeasurementAddedAsync(DeviceMeasurement measurement);
    Task NotifyMeasurementUpdatedAsync(DeviceMeasurement measurement);
    Task NotifyMeasurementDeletedAsync(Guid deviceId, Guid measurementId);
    Task NotifyThresholdExceededAsync(ThresholdAlert alert);
    Task NotifyDeviceStatusChangedAsync(Guid deviceId, bool isActive);
}