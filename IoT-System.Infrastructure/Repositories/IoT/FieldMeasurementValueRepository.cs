using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class FieldMeasurementValueRepository(IoTDbContext context)
    : RepositoryBase<FieldMeasurementValue, IoTDbContext>(context), IFieldMeasurementValueRepository
{
    public Task<OperationResult<List<FieldMeasurementValue>>> GetByMeasurementIdAsync(Guid measurementId)
    {
        return ExecuteAsync(() =>
            _dbSet
                .Include(fv => fv.Field)
                .Where(fv => fv.MeasurementId == measurementId)
                .ToListAsync());
    }

    public Task<OperationResult<List<FieldMeasurementValue>>> GetByFieldIdAsync(
        Guid fieldId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
    {
        return ExecuteAsync(() =>
        {
            var query = _dbSet
                .Include(fv => fv.Measurement)
                .Where(fv => fv.FieldId == fieldId);

            if (startDate.HasValue)
            {
                query = query.Where(fv => fv.Measurement.MeasurementDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(fv => fv.Measurement.MeasurementDate <= endDate.Value);
            }

            query = query.OrderByDescending(fv => fv.Measurement.MeasurementDate);

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return query.ToListAsync();
        });
    }
}