using IoT_System.Application.Interfaces.Repositories.IoT;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Infrastructure.Services.IoT;

public class DeviceAccessPermissionService : IDeviceAccessPermissionService
{
    private readonly IDeviceAccessPermissionRepository _permissionRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IAccessValidationService _accessValidationService;

    public DeviceAccessPermissionService(
        IDeviceAccessPermissionRepository permissionRepository,
        IDeviceRepository deviceRepository,
        IAccessValidationService accessValidationService)
    {
        _permissionRepository = permissionRepository;
        _deviceRepository = deviceRepository;
        _accessValidationService = accessValidationService;
    }

    public async Task<OperationResult<DeviceAccessPermission>> CreateAsync(DeviceAccessPermission permission)
    {
        var deviceResult = await _deviceRepository.GetByIdAsync(permission.DeviceId);
        if (!deviceResult.IsSuccess || deviceResult.Data == null)
        {
            return OperationResult<DeviceAccessPermission>.NotFound($"Device with ID {permission.DeviceId} not found");
        }

        if (!permission.UserId.HasValue && !permission.RoleId.HasValue && !permission.GroupId.HasValue)
        {
            return OperationResult<DeviceAccessPermission>.BadRequest("At least one of UserId, RoleId, or GroupId must be specified");
        }

        permission.CreatedBy = _accessValidationService.GetCurrentUserId();
        permission.CreatedOn = DateTime.UtcNow;

        return await _permissionRepository.AddAsync(permission);
    }

    public async Task<OperationResult<DeviceAccessPermission>> UpdateAsync(DeviceAccessPermission permission)
    {
        var existingResult = await _permissionRepository.GetByIdAsync(permission.Id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return OperationResult<DeviceAccessPermission>.NotFound($"Permission with ID {permission.Id} not found");
        }

        if (!permission.UserId.HasValue && !permission.RoleId.HasValue && !permission.GroupId.HasValue)
        {
            return OperationResult<DeviceAccessPermission>.BadRequest("At least one of UserId, RoleId, or GroupId must be specified");
        }

        var existing = existingResult.Data;
        existing.UserId = permission.UserId;
        existing.RoleId = permission.RoleId;
        existing.GroupId = permission.GroupId;
        existing.PermissionType = permission.PermissionType;
        existing.LastModifiedBy = _accessValidationService.GetCurrentUserId();
        existing.LastModifiedOn = DateTime.UtcNow;

        return await _permissionRepository.UpdateAsync(existing);
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        var permissionResult = await _permissionRepository.GetByIdAsync(id);
        if (!permissionResult.IsSuccess || permissionResult.Data == null)
        {
            return OperationResult.NotFound($"Permission with ID {id} not found");
        }

        return await _permissionRepository.DeleteAsync(permissionResult.Data);
    }

    public Task<OperationResult<DeviceAccessPermission>> GetByIdAsync(Guid id)
    {
        return _permissionRepository.GetByIdAsync(id);
    }

    public Task<OperationResult<List<DeviceAccessPermission>>> GetByDeviceIdAsync(Guid deviceId)
    {
        return _permissionRepository.GetByDeviceIdAsync(deviceId);
    }

    public async Task<OperationResult<bool>> ValidateAccessAsync(Guid deviceId, DevicePermissionType requiredPermission = DevicePermissionType.View)
    {
        var userId = _accessValidationService.GetCurrentUserId();
        var roleIds = _accessValidationService.GetCurrentUserRolesIds();
        var groupIds = _accessValidationService.GetCurrentUserGroupsIds();

        return await _permissionRepository.HasAccessAsync(deviceId, userId, roleIds, groupIds, requiredPermission);
    }
}