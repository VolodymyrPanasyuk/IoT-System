using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class DeviceMeasurementRepository(IoTDbContext context) : RepositoryBase<DeviceMeasurement, IoTDbContext>(context), IDeviceMeasurementRepository
{
    public Task<OperationResult<List<DeviceMeasurement>>> GetByDeviceIdAsync(
        Guid deviceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
    {
        return ExecuteAsync(() =>
        {
            var query = _dbSet
                .Include(m => m.FieldValues)
                .ThenInclude(fv => fv.Field)
                .Where(m => m.DeviceId == deviceId);

            if (startDate.HasValue)
            {
                query = query.Where(m => m.MeasurementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(m => m.MeasurementDate <= endDate.Value);
            }

            query = query.OrderByDescending(m => m.MeasurementDate);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return query.ToListAsync();
        });
    }

    public Task<OperationResult<DeviceMeasurement?>> GetLatestByDeviceIdAsync(Guid deviceId)
    {
        return ExecuteAsync(() =>
            _dbSet
                .Include(m => m.FieldValues)
                .ThenInclude(fv => fv.Field)
                .Where(m => m.DeviceId == deviceId)
                .OrderByDescending(m => m.MeasurementDate)
                .FirstOrDefaultAsync());
    }
}