using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DeviceMeasurementsController : ControllerBase
{
    private readonly IDeviceMeasurementService _measurementService;
    private readonly IDeviceAccessPermissionService _permissionService;

    public DeviceMeasurementsController(
        IDeviceMeasurementService measurementService,
        IDeviceAccessPermissionService permissionService)
    {
        _measurementService = measurementService;
        _permissionService = permissionService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceMeasurement>> GetById(Guid id)
    {
        var result = await _measurementService.GetByIdAsync(id);
        return result.ToResult();
    }

    [HttpGet("device/{deviceId:guid}")]
    public async Task<ActionResult<List<DeviceMeasurement>>> GetByDeviceId(
        Guid deviceId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? limit = null)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(deviceId);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _measurementService.GetByDeviceIdAsync(deviceId, startDate, endDate, limit);
        return result.ToResult();
    }

    [HttpGet("device/{deviceId:guid}/latest")]
    public async Task<ActionResult<DeviceMeasurement>> GetLatestByDeviceId(Guid deviceId)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(deviceId);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _measurementService.GetLatestByDeviceIdAsync(deviceId);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<DeviceMeasurement>> Create([FromBody] DeviceMeasurement measurement)
    {
        var accessResult = await _permissionService.ValidateAccessAsync(measurement.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _measurementService.CreateAsync(measurement);
        return result.ToResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeviceMeasurement>> Update(Guid id, [FromBody] DeviceMeasurement measurement)
    {
        var existingResult = await _measurementService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResult();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        measurement.Id = id;
        var result = await _measurementService.UpdateAsync(measurement);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingResult = await _measurementService.GetByIdAsync(id);
        if (!existingResult.IsSuccess || existingResult.Data == null)
        {
            return existingResult.ToResultUntyped();
        }

        var accessResult = await _permissionService.ValidateAccessAsync(existingResult.Data.DeviceId, DevicePermissionType.Configure);
        if (!accessResult.IsSuccess)
        {
            return accessResult.ToResult();
        }

        var result = await _measurementService.DeleteAsync(id);
        return result.ToResult();
    }
}