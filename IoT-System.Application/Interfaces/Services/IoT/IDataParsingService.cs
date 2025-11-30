using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IDataParsingService
{
    Task<OperationResult<DeviceMeasurement>> ParseAndCreateMeasurementAsync(
        Guid deviceId,
        string rawData,
        DataFormat dataFormat);

    OperationResult<object?> ExtractValue(string rawData, DataFormat dataFormat, string? sourcePath);
    OperationResult<object?> ApplyTransformations(object? value, string? transformationPipeline);
    OperationResult<DateTime> ParseMeasurementDate(string rawData, DataFormat dataFormat, MeasurementDateMapping? mapping);
}