using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class MeasurementDateMappingRepository(IoTDbContext context)
    : RepositoryBase<MeasurementDateMapping, IoTDbContext>(context), IMeasurementDateMappingRepository
{
    public Task<OperationResult<MeasurementDateMapping?>> GetActiveByDeviceAndFormatAsync(Guid deviceId, DataFormat dataFormat)
    {
        return ExecuteAsync(() =>
            _dbSet.FirstOrDefaultAsync(m => m.DeviceId == deviceId && m.DataFormat == dataFormat && m.IsActive));
    }
}