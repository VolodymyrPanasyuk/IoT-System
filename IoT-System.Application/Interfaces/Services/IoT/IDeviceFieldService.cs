using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IDeviceFieldService
{
    Task<OperationResult<DeviceField>> CreateAsync(DeviceField field);
    Task<OperationResult<DeviceField>> UpdateAsync(DeviceField field);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<DeviceField>> GetByIdAsync(Guid id);
    Task<OperationResult<List<DeviceField>>> GetByDeviceIdAsync(Guid deviceId, bool activeOnly = true);
    Task<OperationResult<IEnumerable<DeviceField>>> GetAllAsync();
}