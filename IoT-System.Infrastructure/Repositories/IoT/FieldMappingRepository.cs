using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using IoT_System.Infrastructure.Contexts;
using IoT_System.Infrastructure.Repositories.Auth;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Repositories.IoT;

public class FieldMappingRepository(IoTDbContext context) : RepositoryBase<FieldMapping, IoTDbContext>(context), IFieldMappingRepository
{
    public Task<OperationResult<FieldMapping?>> GetActiveByFieldAndFormatAsync(Guid fieldId, DataFormat dataFormat)
    {
        return ExecuteAsync(() =>
            _dbSet.FirstOrDefaultAsync(m => m.FieldId == fieldId && m.DataFormat == dataFormat && m.IsActive));
    }

    public Task<OperationResult<List<FieldMapping>>> GetByFieldIdAsync(Guid fieldId)
    {
        return ExecuteAsync(() => _dbSet.Where(m => m.FieldId == fieldId).ToListAsync());
    }
}