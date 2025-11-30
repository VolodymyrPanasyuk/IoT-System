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
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDeviceAccessPermissionService _permissionService;
    private readonly IIoTHubService _hubService;

    public DevicesController(
        IDeviceService deviceService,
        IDeviceAccessPermissionService permissionService,
        IIoTHubService hubService)
    {
        _deviceService = deviceService;
        _permissionService = permissionService;
        _hubService = hubService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetAll([FromQuery] bool includeRelations = false, [FromQuery] bool activeOnly = false)
    {
        var result = await _deviceService.GetAllAsync(includeRelations, activeOnly);
        return result.ToResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Device>> GetById(Guid id, [FromQuery] bool includeRelations = false)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(id);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToOperationResult().ToResult();
        }

        var result = await _deviceService.GetByIdAsync(id, includeRelations);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<Device>> Create([FromBody] Device device)
    {
        var result = await _deviceService.CreateAsync(device);
        return result.ToResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Device>> Update(Guid id, [FromBody] Device device)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(id, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        device.Id = id;
        var result = await _deviceService.UpdateAsync(device);

        if (result.IsSuccess && result.Data != null)
        {
            await _hubService.NotifyDeviceStatusChangedAsync(result.Data.Id, result.Data.IsActive);
        }

        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(id, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _deviceService.DeleteAsync(id);
        return result.ToResult();
    }

    [HttpPost("generate-api-key")]
    public ActionResult<string> GenerateApiKey()
    {
        var result = _deviceService.GenerateApiKey();
        return result.ToResult();
    }
}