using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Infrastructure.Services.IoT;

public class DeviceFieldService : IDeviceFieldService
{
    private readonly IDeviceFieldRepository _fieldRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAccessValidationService _accessValidationService;

    public DeviceFieldService(
        IDeviceFieldRepository fieldRepository,
        IDeviceRepository deviceRepository,
        IAccessValidationService accessValidationService)
    {
        _fieldRepository = fieldRepository;
        _deviceRepository = deviceRepository;
        _accessValidationService = accessValidationService;
    }

    public async Task<OperationResult<DeviceField>> CreateAsync(DeviceField field)
    {
        var deviceResult = await _deviceRepository.GetByIdAsync(field.DeviceId);
        if (!deviceResult.IsSuccess || deviceResult.Data == null)
        {
            return OperationResult<DeviceField>.NotFound($"Device with ID {field.DeviceId} not found");
        }

        var isUniqueResult = await _fieldRepository.IsFieldNameUniqueAsync(field.DeviceId, field.FieldName);
        if (!isUniqueResult.IsSuccess) return isUniqueResult.ToOperationResult<DeviceField>();
        if (!isUniqueResult.Data)
        {
            return OperationResult<DeviceField>.Conflict($"Field with name '{field.FieldName}' already exists for this device");
        }

        field.CreatedBy = _accessValidationService.GetCurrentUserId();
        field.CreatedOn = DateTime.UtcNow;

        return await _fieldRepository.AddAsync(field);
    }

    public async Task<OperationResult<DeviceField>> UpdateAsync(DeviceField field)
    {
        var existingResult = await _fieldRepository.GetByIdAsync(field.Id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return OperationResult<DeviceField>.NotFound($"Field with ID {field.Id} not found");
        }

        var isUniqueResult = await _fieldRepository.IsFieldNameUniqueAsync(field.DeviceId, field.FieldName, field.Id);
        if (!isUniqueResult.IsSuccess) return isUniqueResult.ToOperationResult<DeviceField>();
        if (!isUniqueResult.Data)
        {
            return OperationResult<DeviceField>.Conflict($"Field with name '{field.FieldName}' already exists for this device");
        }

        var existing = existingResult.Data;
        existing.FieldName = field.FieldName;
        existing.DisplayName = field.DisplayName;
        existing.Description = field.Description;
        existing.DataType = field.DataType;
        existing.Unit = field.Unit;
        existing.DisplayOrder = field.DisplayOrder;
        existing.IsActive = field.IsActive;
        existing.WarningMin = field.WarningMin;
        existing.WarningMax = field.WarningMax;
        existing.CriticalMin = field.CriticalMin;
        existing.CriticalMax = field.CriticalMax;
        existing.LastModifiedBy = _accessValidationService.GetCurrentUserId();
        existing.LastModifiedOn = DateTime.UtcNow;

        return await _fieldRepository.UpdateAsync(existing);
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var fieldResult = await _fieldRepository.GetByIdAsync(id);
        if (!fieldResult.IsSuccess || fieldResult.Data == null)
        {
            return OperationResult.NotFound($"Field with ID {id} not found");
        }

        return await _fieldRepository.DeleteAsync(fieldResult.Data);
    }

    public Task<OperationResult<DeviceField>> GetByIdAsync(Guid id)
    {
        return _fieldRepository.GetByIdAsync(id);
    }

    public Task<OperationResult<List<DeviceField>>> GetByDeviceIdAsync(Guid deviceId, bool activeOnly = true)
    {
        return _fieldRepository.GetByDeviceIdAsync(deviceId, activeOnly);
    }

    public async Task<OperationResult<IEnumerable<DeviceField>>> GetAllAsync()
    {
        var result = await _fieldRepository.GetAllAsync();
        return result;
    }
}