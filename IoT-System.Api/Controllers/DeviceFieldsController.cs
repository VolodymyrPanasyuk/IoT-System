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
public class DeviceFieldsController : ControllerBase
{
    private readonly IDeviceFieldService _fieldService;
    private readonly IDeviceAccessPermissionService _permissionService;

    public DeviceFieldsController(
        IDeviceFieldService fieldService,
        IDeviceAccessPermissionService permissionService)
    {
        _fieldService = fieldService;
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceField>>> GetAll()
    {
        var result = await _fieldService.GetAllAsync();
        return result.ToResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceField>> GetById(Guid id)
    {
        var result = await _fieldService.GetByIdAsync(id);
        return result.ToResult();
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<List<DeviceField>>> GetByDeviceId(Guid deviceId, [FromQuery] bool activeOnly = true)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(deviceId);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _fieldService.GetByDeviceIdAsync(deviceId, activeOnly);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<DeviceField>> Create([FromBody] DeviceField field)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(field.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _fieldService.CreateAsync(field);
        return result.ToResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeviceField>> Update(Guid id, [FromBody] DeviceField field)
    {
        var existingResult = await _fieldService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResult();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        field.Id = id;
        var result = await _fieldService.UpdateAsync(field);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingResult = await _fieldService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _fieldService.DeleteAsync(id);
        return result.ToResult();
    }
}