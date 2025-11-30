using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IFieldMappingService
{
    Task<OperationResult<FieldMapping>> CreateAsync(FieldMapping mapping);
    Task<OperationResult<FieldMapping>> UpdateAsync(FieldMapping mapping);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<FieldMapping>> GetByIdAsync(Guid id);
    Task<OperationResult<List<FieldMapping>>> GetByFieldIdAsync(Guid fieldId);
}