using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Application.Interfaces.Repositories.IoT;

public interface IMeasurementDateMappingRepository : IRepositoryBase<MeasurementDateMapping>
{
    Task<OperationResult<MeasurementDateMapping?>> GetActiveByDeviceAndFormatAsync(Guid deviceId, DataFormat dataFormat);
}