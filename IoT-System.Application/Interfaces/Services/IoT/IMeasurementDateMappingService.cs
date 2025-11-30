using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IMeasurementDateMappingService
{
    Task<OperationResult<MeasurementDateMapping>> CreateAsync(MeasurementDateMapping mapping);
    Task<OperationResult<MeasurementDateMapping>> UpdateAsync(MeasurementDateMapping mapping);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<MeasurementDateMapping>> GetByIdAsync(Guid id);
}