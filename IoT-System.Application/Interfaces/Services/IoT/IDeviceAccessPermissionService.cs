using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IDeviceAccessPermissionService
{
    Task<OperationResult<DeviceAccessPermission>> CreateAsync(DeviceAccessPermission permission);
    Task<OperationResult<DeviceAccessPermission>> UpdateAsync(DeviceAccessPermission permission);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<DeviceAccessPermission>> GetByIdAsync(Guid id);
    Task<OperationResult<List<DeviceAccessPermission>>> GetByDeviceIdAsync(Guid deviceId);
    Task<OperationResult<bool>> ValidateAccessAsync(Guid deviceId, DevicePermissionType requiredPermission = DevicePermissionType.View);
}