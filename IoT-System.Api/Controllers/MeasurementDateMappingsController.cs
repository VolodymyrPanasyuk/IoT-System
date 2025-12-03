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
public class MeasurementDateMappingsController : ControllerBase
{
    private readonly IMeasurementDateMappingService _mappingService;
    private readonly IDeviceAccessPermissionService _permissionService;

    public MeasurementDateMappingsController(
        IMeasurementDateMappingService mappingService,
        IDeviceAccessPermissionService permissionService)
    {
        _mappingService = mappingService;
        _permissionService = permissionService;
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<MeasurementDateMapping>> GetById(Guid id)
    {
        var result = await _mappingService.GetByIdAsync(id);
        return result.ToResult();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<MeasurementDateMapping>> Create([FromBody] MeasurementDateMapping mapping)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(mapping.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _mappingService.CreateAsync(mapping);
        return result.ToResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MeasurementDateMapping>> Update(Guid id, [FromBody] MeasurementDateMapping mapping)
    {
        var existingResult = await _mappingService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResult();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        mapping.Id = id;
        var result = await _mappingService.UpdateAsync(mapping);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingResult = await _mappingService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _mappingService.DeleteAsync(id);
        return result.ToResult();
    }
}