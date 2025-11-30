using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IFieldMappingRepository : IRepositoryBase<FieldMapping>
{
    Task<OperationResult<FieldMapping?>> GetActiveByFieldAndFormatAsync(Guid fieldId, DataFormat dataFormat);
    Task<OperationResult<List<FieldMapping>>> GetByFieldIdAsync(Guid fieldId);
}