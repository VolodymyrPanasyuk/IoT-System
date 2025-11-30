using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Infrastructure.Services.IoT;

public class FieldMappingService : IFieldMappingService
{
    private readonly IFieldMappingRepository _mappingRepository;
    private readonly IDeviceFieldRepository _fieldRepository;
    private readonly IAccessValidationService _accessValidationService;

    public FieldMappingService(
        IFieldMappingRepository mappingRepository,
        IDeviceFieldRepository fieldRepository,
        IAccessValidationService accessValidationService)
    {
        _mappingRepository = mappingRepository;
        _fieldRepository = fieldRepository;
        _accessValidationService = accessValidationService;
    }

    public async Task<OperationResult<FieldMapping>> CreateAsync(FieldMapping mapping)
    {
        var fieldResult = await _fieldRepository.GetByIdAsync(mapping.FieldId);
        if (!fieldResult.IsSuccess || fieldResult.Data == null)
        {
            return OperationResult<FieldMapping>.NotFound($"Field with ID {mapping.FieldId} not found");
        }

        mapping.CreatedBy = _accessValidationService.GetCurrentUserId();
        mapping.CreatedOn = DateTime.UtcNow;

        return await _mappingRepository.AddAsync(mapping);
    }

    public async Task<OperationResult<FieldMapping>> UpdateAsync(FieldMapping mapping)
    {
        var existingResult = await _mappingRepository.GetByIdAsync(mapping.Id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return OperationResult<FieldMapping>.NotFound($"Mapping with ID {mapping.Id} not found");
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

    public Task<OperationResult<FieldMapping>> GetByIdAsync(Guid id)
    {
        return _mappingRepository.GetByIdAsync(id);
    }

    public Task<OperationResult<List<FieldMapping>>> GetByFieldIdAsync(Guid fieldId)
    {
        return _mappingRepository.GetByFieldIdAsync(fieldId);
    }
}