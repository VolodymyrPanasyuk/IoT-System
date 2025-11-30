using System.Security.Cryptography;
using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using Microsoft.EntityFrameworkCore;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Services.IoT;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAccessValidationService _accessValidationService;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IAccessValidationService accessValidationService)
    {
        _deviceRepository = deviceRepository;
        _accessValidationService = accessValidationService;
    }

    public async Task<OperationResult<Device>> CreateAsync(Device device)
    {
        var isUniqueResult = await _deviceRepository.IsApiKeyUniqueAsync(device.ApiKey);
        if (!isUniqueResult.IsSuccess) return isUniqueResult.ToOperationResult<Device>();
        if (!isUniqueResult.Data)
        {
            return OperationResult<Device>.Conflict("API Key already exists");
        }

        device.CreatedBy = _accessValidationService.GetCurrentUserId();
        device.CreatedOn = DateTime.UtcNow;

        return await _deviceRepository.AddAsync(device);
    }

    public async Task<OperationResult<Device>> UpdateAsync(Device device)
    {
        var existingResult = await _deviceRepository.GetByIdAsync(device.Id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return OperationResult<Device>.NotFound($"Device with ID {device.Id} not found");
        }

        var isUniqueResult = await _deviceRepository.IsApiKeyUniqueAsync(device.ApiKey, device.Id);
        if (!isUniqueResult.IsSuccess) return isUniqueResult.ToOperationResult<Device>();
        if (!isUniqueResult.Data)
        {
            return OperationResult<Device>.Conflict("API Key already exists");
        }

        var existing = existingResult.Data;
        existing.Name = device.Name;
        existing.Description = device.Description;
        existing.Latitude = device.Latitude;
        existing.Longitude = device.Longitude;
        existing.IsActive = device.IsActive;
        existing.ApiKey = device.ApiKey;
        existing.ApiKeyExpiresAt = device.ApiKeyExpiresAt;
        existing.LastModifiedBy = _accessValidationService.GetCurrentUserId();
        existing.LastModifiedOn = DateTime.UtcNow;

        return await _deviceRepository.UpdateAsync(existing);
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var deviceResult = await _deviceRepository.GetByIdAsync(id);
        if (!deviceResult.IsSuccess || deviceResult.Data == null)
        {
            return OperationResult.NotFound($"Device with ID {id} not found");
        }

        return await _deviceRepository.DeleteAsync(deviceResult.Data);
    }

    public async Task<OperationResult<Device>> GetByIdAsync(Guid id, bool includeRelations = false)
    {
        if (!includeRelations)
        {
            return await _deviceRepository.GetByIdAsync(id);
        }

        return await ExecuteAsync(async Task<Device> () =>
        {
            var device = await _deviceRepository.AsQueryable()
                .Include(d => d.Fields.Where(f => f.IsActive))
                .Include(d => d.AccessPermissions)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (device == null)
            {
                return OperationResult<Device>.NotFound($"Device with ID {id} not found");
            }

            return device;
        });
    }

    public async Task<OperationResult<IEnumerable<Device>>> GetAllAsync(bool includeRelations = false, bool activeOnly = false)
    {
        return await ExecuteAsync(async () =>
        {
            var query = _deviceRepository.AsQueryable();

            if (activeOnly)
            {
                query = query.Where(d => d.IsActive);
            }

            if (includeRelations)
            {
                query = query
                    .Include(d => d.Fields.Where(f => f.IsActive))
                    .Include(d => d.AccessPermissions);
            }

            return (IEnumerable<Device>)await query.OrderBy(d => d.Name).ToListAsync();
        });
    }

    public Task<OperationResult<Device>> GetByApiKeyAsync(string apiKey)
    {
        return _deviceRepository.GetByApiKeyAsync(apiKey);
    }

    public OperationResult<string> GenerateApiKey()
    {
        return Execute(() =>
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        });
    }

    public async Task<OperationResult> ValidateApiKeyAsync(string apiKey, Guid deviceId)
    {
        var deviceResult = await _deviceRepository.GetByApiKeyAsync(apiKey);
        if (!deviceResult.IsSuccess || deviceResult.Data == null)
        {
            return OperationResult.Unauthorized("Invalid API Key");
        }

        var device = deviceResult.Data;

        if (device.Id != deviceId)
        {
            return OperationResult.Forbidden("API Key does not match device");
        }

        if (!device.IsActive)
        {
            return OperationResult.Forbidden("Device is not active");
        }

        if (device.ApiKeyExpiresAt.HasValue && device.ApiKeyExpiresAt.Value < DateTime.UtcNow)
        {
            return OperationResult.Unauthorized("API Key has expired");
        }

        return OperationResult.Success();
    }
}