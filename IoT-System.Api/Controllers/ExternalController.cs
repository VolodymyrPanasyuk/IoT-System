using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Domain.Entities.IoT.Enums;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route(Constants.ApiRoutes.External)]
[ApiExplorerSettings(GroupName = Constants.SwaggerGroups.External)]
[Produces("application/json")]
public class ExternalController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly IDataParsingService _parsingService;
    private readonly IDeviceMeasurementService _measurementService;

    public ExternalController(
        IDeviceService deviceService,
        IDataParsingService parsingService,
        IDeviceMeasurementService measurementService)
    {
        _deviceService = deviceService;
        _parsingService = parsingService;
        _measurementService = measurementService;
    }

    [HttpPost("devices/{deviceId:guid}/measurements")]
    public async Task<IActionResult> AddMeasurement(
        Guid deviceId,
        [FromBody] ExternalMeasurementRequest request)
    {
        // Validate API Key from header
        if (!Request.Headers.TryGetValue(Constants.ApiHeaders.ApiKey, out var apiKeyHeader))
        {
            return Unauthorized(new { message = "API Key is required" });
        }

        var apiKey = apiKeyHeader.ToString();
        var validationResult = await _deviceService.ValidateApiKeyAsync(apiKey, deviceId);

        if (!validationResult.IsSuccess)
        {
            return validationResult.ToResult();
        }

        // Parse and create measurement
        var parseResult = await _parsingService.ParseAndCreateMeasurementAsync(
            deviceId,
            request.RawData,
            request.DataFormat);

        if (!parseResult.IsSuccess)
        {
            return parseResult.ToResultUntyped();
        }

        var createResult = await _measurementService.CreateAsync(parseResult.Data!);
        return createResult.ToResultUntyped();
    }
}

public record ExternalMeasurementRequest(string RawData, DataFormat DataFormat);