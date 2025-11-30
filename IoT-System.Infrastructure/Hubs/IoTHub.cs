using IoT_System.Application.Common;
using IoT_System.Application.Interfaces.Services.IoT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace IoT_System.Infrastructure.Hubs;

[Authorize]
public class IoTHub : Hub
{
    private readonly IDeviceAccessPermissionService _permissionService;

    public IoTHub(IDeviceAccessPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public override async Task OnConnectedAsync()
    {
        // Add to global group
        await Groups.AddToGroupAsync(Context.ConnectionId, Constants.SignalR.Groups.AllDevices);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants.SignalR.Groups.AllDevices);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToDevice(Guid deviceId)
    {
        // Validate access
        var hasAccess = await _permissionService.ValidateAccessAsync(deviceId);
        if (hasAccess.IsSuccess)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Constants.SignalR.Groups.DeviceGroup(deviceId));
        }
    }

    public async Task UnsubscribeFromDevice(Guid deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants.SignalR.Groups.DeviceGroup(deviceId));
    }
}