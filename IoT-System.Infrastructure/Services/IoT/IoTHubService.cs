using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Services.IoT;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.IoT;
using IoT_System.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IoT_System.Infrastructure.Services.IoT;

public class IoTHubService : IIoTHubService
{
    private readonly IHubContext<IoTHub> _hubContext;

    public IoTHubService(IHubContext<IoTHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMeasurementAddedAsync(DeviceMeasurement measurement)
    {
        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.DeviceGroup(measurement.DeviceId))
            .SendAsync(Constants.SignalR.HubMethods.MeasurementAdded, measurement);

        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.AllDevices)
            .SendAsync(Constants.SignalR.HubMethods.MeasurementAdded, measurement);
    }

    public async Task NotifyMeasurementUpdatedAsync(DeviceMeasurement measurement)
    {
        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.DeviceGroup(measurement.DeviceId))
            .SendAsync(Constants.SignalR.HubMethods.MeasurementUpdated, measurement);

        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.AllDevices)
            .SendAsync(Constants.SignalR.HubMethods.MeasurementUpdated, measurement);
    }

    public async Task NotifyMeasurementDeletedAsync(Guid deviceId, Guid measurementId)
    {
        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.DeviceGroup(deviceId))
            .SendAsync(Constants.SignalR.HubMethods.MeasurementDeleted, new { deviceId, measurementId });

        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.AllDevices)
            .SendAsync(Constants.SignalR.HubMethods.MeasurementDeleted, new { deviceId, measurementId });
    }

    public async Task NotifyThresholdExceededAsync(ThresholdAlert alert)
    {
        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.DeviceGroup(alert.DeviceId))
            .SendAsync(Constants.SignalR.HubMethods.ThresholdExceeded, alert);

        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.AllDevices)
            .SendAsync(Constants.SignalR.HubMethods.ThresholdExceeded, alert);
    }

    public async Task NotifyDeviceStatusChangedAsync(Guid deviceId, bool isActive)
    {
        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.DeviceGroup(deviceId))
            .SendAsync(Constants.SignalR.HubMethods.DeviceStatusChanged, new { deviceId, isActive });

        await _hubContext.Clients
            .Group(Constants.SignalR.Groups.AllDevices)
            .SendAsync(Constants.SignalR.HubMethods.DeviceStatusChanged, new { deviceId, isActive });
    }
}