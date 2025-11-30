using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class DeviceAccessPermissionRepository(IoTDbContext context)
    : RepositoryBase<DeviceAccessPermission, IoTDbContext>(context), IDeviceAccessPermissionRepository
{
    public Task<OperationResult<bool>> HasAccessAsync(
        Guid deviceId,
        Guid? userId = null,
        List<Guid>? roleIds = null,
        List<Guid>? groupIds = null,
        DevicePermissionType requiredPermission = DevicePermissionType.View)
    {
        return ExecuteAsync(async () =>
        {
            var query = _dbSet.Where(p => p.DeviceId == deviceId);

            // Filter by permission type (View or Configure)
            // Configure permission includes View
            if (requiredPermission == DevicePermissionType.Configure)
            {
                query = query.Where(p => p.PermissionType == DevicePermissionType.Configure);
            }

            // Check if any permission matches user, role, or group
            var hasAccess = await query.AnyAsync(p =>
                (userId.HasValue && p.UserId == userId.Value) ||
                (roleIds != null && roleIds.Any() && p.RoleId != null && roleIds.Contains(p.RoleId.Value)) ||
                (groupIds != null && groupIds.Any() && p.GroupId != null && groupIds.Contains(p.GroupId.Value))
            );

            return hasAccess;
        });
    }

    public Task<OperationResult<List<DeviceAccessPermission>>> GetByDeviceIdAsync(Guid deviceId)
    {
        return ExecuteAsync(() => _dbSet.Where(p => p.DeviceId == deviceId).ToListAsync());
    }
}