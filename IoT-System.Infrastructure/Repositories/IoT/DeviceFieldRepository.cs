using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class DeviceFieldRepository(IoTDbContext context) : RepositoryBase<DeviceField, IoTDbContext>(context), IDeviceFieldRepository
{
    public Task<OperationResult<List<DeviceField>>> GetByDeviceIdAsync(Guid deviceId, bool activeOnly = true)
    {
        return ExecuteAsync(() =>
        {
            var query = _dbSet.Where(f => f.DeviceId == deviceId);
            if (activeOnly)
            {
                query = query.Where(f => f.IsActive);
            }

            return query.OrderBy(f => f.DisplayOrder).ToListAsync();
        });
    }

    public Task<OperationResult<bool>> IsFieldNameUniqueAsync(Guid deviceId, string fieldName, Guid? excludeFieldId = null)
    {
        return ExecuteAsync(async () =>
        {
            var query = _dbSet.Where(f => f.DeviceId == deviceId && f.FieldName == fieldName);
            if (excludeFieldId.HasValue)
            {
                query = query.Where(f => f.Id != excludeFieldId.Value);
            }

            return !await query.AnyAsync();
        });
    }
}