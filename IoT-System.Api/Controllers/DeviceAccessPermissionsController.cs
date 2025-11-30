using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route($"{Constants.ApiRoutes.System}/[controller]")]
[ApiExplorerSettings(GroupName = Constants.SwaggerGroups.System)]
[Produces("application/json")]
public class DeviceAccessPermissionsController : ControllerBase
{
    private readonly IDeviceAccessPermissionService _permissionService;

    public DeviceAccessPermissionsController(IDeviceAccessPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceAccessPermission>> GetById(Guid id)
    {
        var result = await _permissionService.GetByIdAsync(id);
        return result.ToResult();
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<List<DeviceAccessPermission>>> GetByDeviceId(Guid deviceId)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(deviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _permissionService.GetByDeviceIdAsync(deviceId);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<DeviceAccessPermission>> Create([FromBody] DeviceAccessPermission permission)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(permission.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _permissionService.CreateAsync(permission);
        return result.ToResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeviceAccessPermission>> Update(Guid id, [FromBody] DeviceAccessPermission permission)
    {
        var existingResult = await _permissionService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResult();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        permission.Id = id;
        var result = await _permissionService.UpdateAsync(permission);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingResult = await _permissionService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _permissionService.DeleteAsync(id);
        return result.ToResult();
    }
}