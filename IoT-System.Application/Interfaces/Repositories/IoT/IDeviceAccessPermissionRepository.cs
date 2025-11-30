using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IDeviceAccessPermissionRepository : IRepositoryBase<DeviceAccessPermission>
{
    Task<OperationResult<bool>> HasAccessAsync(
        Guid deviceId,
        Guid? userId = null,
        List<Guid>? roleIds = null,
        List<Guid>? groupIds = null,
        DevicePermissionType requiredPermission = DevicePermissionType.View);

    Task<OperationResult<List<DeviceAccessPermission>>> GetByDeviceIdAsync(Guid deviceId);
}