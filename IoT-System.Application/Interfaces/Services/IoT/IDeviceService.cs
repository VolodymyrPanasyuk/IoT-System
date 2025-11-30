using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;

namespace IoT_System.Application.Interfaces.Services.IoT;

public interface IDeviceService
{
    Task<OperationResult<Device>> CreateAsync(Device device);
    Task<OperationResult<Device>> UpdateAsync(Device device);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<Device>> GetByIdAsync(Guid id, bool includeRelations = false);
    Task<OperationResult<IEnumerable<Device>>> GetAllAsync(bool includeRelations = false, bool activeOnly = false);
    Task<OperationResult<Device>> GetByApiKeyAsync(string apiKey);
    OperationResult<string> GenerateApiKey();
    Task<OperationResult> ValidateApiKeyAsync(string apiKey, Guid deviceId);
}