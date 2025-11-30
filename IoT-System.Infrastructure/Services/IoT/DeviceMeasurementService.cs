using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Infrastructure.Services.IoT;

public class DeviceMeasurementService : IDeviceMeasurementService
{
    private readonly IDeviceMeasurementRepository _measurementRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IThresholdService _thresholdService;
    private readonly IIoTHubService _hubService;

    public DeviceMeasurementService(
        IDeviceMeasurementRepository measurementRepository,
        IDeviceRepository deviceRepository,
        IThresholdService thresholdService,
        IIoTHubService hubService)
    {
        _measurementRepository = measurementRepository;
        _deviceRepository = deviceRepository;
        _thresholdService = thresholdService;
        _hubService = hubService;
    }

    public async Task<OperationResult<DeviceMeasurement>> CreateAsync(DeviceMeasurement measurement)
    {
        var deviceResult = await _deviceRepository.GetByIdAsync(measurement.DeviceId);
        if (!deviceResult.IsSuccess || deviceResult.Data == null)
        {
            return OperationResult<DeviceMeasurement>.NotFound($"Device with ID {measurement.DeviceId} not found");
        }

        measurement.CreatedOn = DateTime.UtcNow;
        if (measurement.MeasurementDate == default)
        {
            measurement.MeasurementDate = measurement.CreatedOn;
        }

        var result = await _measurementRepository.AddAsync(measurement);

        if (result.IsSuccess && result.Data != null)
        {
            // Check thresholds
            var thresholdResult = await _thresholdService.CheckThresholdsAsync(result.Data);
            if (thresholdResult.IsSuccess && thresholdResult.Data != null && thresholdResult.Data.Any())
            {
                var alert = new ThresholdAlert
                {
                    DeviceId = result.Data.DeviceId,
                    DeviceName = deviceResult.Data.Name,
                    MeasurementId = result.Data.Id,
                    MeasurementDate = result.Data.MeasurementDate,
                    ExceededThresholds = thresholdResult.Data
                };
                await _hubService.NotifyThresholdExceededAsync(alert);
            }

            // Notify measurement added
            await _hubService.NotifyMeasurementAddedAsync(result.Data);
        }

        return result;
    }

    public async Task<OperationResult<DeviceMeasurement>> UpdateAsync(DeviceMeasurement measurement)
    {
        var existingResult = await _measurementRepository.GetByIdAsync(measurement.Id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return OperationResult<DeviceMeasurement>.NotFound($"Measurement with ID {measurement.Id} not found");
        }

        var existing = existingResult.Data;
        existing.RawData = measurement.RawData;
        existing.DataFormat = measurement.DataFormat;
        existing.MeasurementDate = measurement.MeasurementDate;
        existing.ParsedSuccessfully = measurement.ParsedSuccessfully;
        existing.ParsingErrors = measurement.ParsingErrors;

        var result = await _measurementRepository.UpdateAsync(existing);

        if (result.IsSuccess && result.Data != null)
        {
            await _hubService.NotifyMeasurementUpdatedAsync(result.Data);
        }

        return result;
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var measurementResult = await _measurementRepository.GetByIdAsync(id);
        if (!measurementResult.IsSuccess || measurementResult.Data == null)
        {
            return OperationResult.NotFound($"Measurement with ID {id} not found");
        }

        var deviceId = measurementResult.Data.DeviceId;
        var result = await _measurementRepository.DeleteAsync(measurementResult.Data);

        if (result.IsSuccess)
        {
            await _hubService.NotifyMeasurementDeletedAsync(deviceId, id);
        }

        return result;
    }

    public Task<OperationResult<DeviceMeasurement>> GetByIdAsync(Guid id)
    {
        return _measurementRepository.GetByIdAsync(id);
    }

    public Task<OperationResult<List<DeviceMeasurement>>> GetByDeviceIdAsync(
        Guid deviceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? limit = null)
    {
        return _measurementRepository.GetByDeviceIdAsync(deviceId, startDate, endDate, limit);
    }

    public Task<OperationResult<DeviceMeasurement>> GetLatestByDeviceIdAsync(Guid deviceId)
    {
        return _measurementRepository.GetLatestByDeviceIdAsync(deviceId);
    }
}