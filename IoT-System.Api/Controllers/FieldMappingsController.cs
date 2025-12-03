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
public class FieldMappingsController : ControllerBase
{
    private readonly IFieldMappingService _mappingService;
    private readonly IDeviceFieldService _fieldService;
    private readonly IDeviceAccessPermissionService _permissionService;

    public FieldMappingsController(
        IFieldMappingService mappingService,
        IDeviceFieldService fieldService,
        IDeviceAccessPermissionService permissionService)
    {
        _mappingService = mappingService;
        _fieldService = fieldService;
        _permissionService = permissionService;
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<FieldMapping>> GetById(Guid id)
    {
        var result = await _mappingService.GetByIdAsync(id);
        return result.ToResult();
    }

    [HttpGet("field/{fieldId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<FieldMapping>>> GetByFieldId(Guid fieldId)
    {
        var result = await _mappingService.GetByFieldIdAsync(fieldId);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<FieldMapping>> Create([FromBody] FieldMapping mapping)
    {
        var fieldResult = await _fieldService.GetByIdAsync(mapping.FieldId);
        if (!fieldResult.IsSuccess || fieldResult.Data == null)
        {
            return fieldResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(fieldResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _mappingService.CreateAsync(mapping);
        return result.ToResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FieldMapping>> Update(Guid id, [FromBody] FieldMapping mapping)
    {
        var existingResult = await _mappingService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResult();
        }

        var fieldResult = await _fieldService.GetByIdAsync(existingResult.Data.FieldId);
        if (!fieldResult.IsSuccess || fieldResult.Data == null)
        {
            return fieldResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(fieldResult.Data.DeviceId, DevicePermissionType.Configure);
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

        var fieldResult = await _fieldService.GetByIdAsync(existingResult.Data.FieldId);
        if (!fieldResult.IsSuccess || fieldResult.Data == null)
        {
            return fieldResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(fieldResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _mappingService.DeleteAsync(id);
        return result.ToResult();
    }
}