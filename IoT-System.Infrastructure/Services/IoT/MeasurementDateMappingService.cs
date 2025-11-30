using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Infrastructure.Services.IoT;

public class MeasurementDateMappingService : IMeasurementDateMappingService
{
    private readonly IMeasurementDateMappingRepository _mappingRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAccessValidationService _accessValidationService;

    public MeasurementDateMappingService(
        IMeasurementDateMappingRepository mappingRepository,
        IDeviceRepository deviceRepository,
        IAccessValidationService accessValidationService)
    {
        _mappingRepository = mappingRepository;
        _deviceRepository = deviceRepository;
        _accessValidationService = accessValidationService;
    }

    public async Task<OperationResult<MeasurementDateMapping>> CreateAsync(MeasurementDateMapping mapping)
    {
        var deviceResult = await _deviceRepository.GetByIdAsync(mapping.DeviceId);
        if (!deviceResult.IsSuccess || deviceResult.Data == null)
        {
            return OperationResult<MeasurementDateMapping>.NotFound($"Device with ID {mapping.DeviceId} not found");
        }

        mapping.CreatedBy = _accessValidationService.GetCurrentUserId();
        mapping.CreatedOn = DateTime.UtcNow;

        return await _mappingRepository.AddAsync(mapping);
    }

    public async Task<OperationResult<MeasurementDateMapping>> UpdateAsync(MeasurementDateMapping mapping)
    {
        var existingResult = await _mappingRepository.GetByIdAsync(mapping.Id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return OperationResult<MeasurementDateMapping>.NotFound($"Mapping with ID {mapping.Id} not found");
        }

        var existing = existingResult.Data;
        existing.DataFormat = mapping.DataFormat;
        existing.SourceFieldPath = mapping.SourceFieldPath;
        existing.TransformationPipeline = mapping.TransformationPipeline;
        existing.IsActive = mapping.IsActive;
        existing.LastModifiedBy = _accessValidationService.GetCurrentUserId();
        existing.LastModifiedOn = DateTime.UtcNow;

        return await _mappingRepository.UpdateAsync(existing);
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var mappingResult = await _mappingRepository.GetByIdAsync(id);
        if (!mappingResult.IsSuccess || mappingResult.Data == null)
        {
            return OperationResult.NotFound($"Mapping with ID {id} not found");
        }

        return await _mappingRepository.DeleteAsync(mappingResult.Data);
    }

    public Task<OperationResult<MeasurementDateMapping>> GetByIdAsync(Guid id)
    {
        return _mappingRepository.GetByIdAsync(id);
    }
}