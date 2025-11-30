using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IFieldMeasurementValueRepository : IRepositoryBase<FieldMeasurementValue>
{
    Task<OperationResult<List<FieldMeasurementValue>>> GetByMeasurementIdAsync(Guid measurementId);

    Task<OperationResult<List<FieldMeasurementValue>>> GetByFieldIdAsync(
        Guid fieldId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null);
}