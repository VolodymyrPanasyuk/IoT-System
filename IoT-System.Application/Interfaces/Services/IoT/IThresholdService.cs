using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IThresholdService
{
    Task<OperationResult<List<ThresholdStatus>>> CheckThresholdsAsync(DeviceMeasurement measurement);
    string GetThresholdStatus(DeviceField field, decimal? value);
}