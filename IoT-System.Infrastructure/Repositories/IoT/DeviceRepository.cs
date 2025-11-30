using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class DeviceRepository(IoTDbContext context) : RepositoryBase<Device, IoTDbContext>(context), IDeviceRepository
{
    public Task<OperationResult<Device?>> GetByApiKeyAsync(string apiKey)
    {
        return ExecuteAsync(() => _dbSet.FirstOrDefaultAsync(d => d.ApiKey == apiKey));
    }

    public Task<OperationResult<bool>> IsApiKeyUniqueAsync(string apiKey, Guid? excludeDeviceId = null)
    {
        return ExecuteAsync(async () =>
        {
            var query = _dbSet.Where(d => d.ApiKey == apiKey);
            if (excludeDeviceId.HasValue)
            {
                query = query.Where(d => d.Id != excludeDeviceId.Value);
            }

            return !await query.AnyAsync();
        });
    }
}