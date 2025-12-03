using IoT_System.Application.Common;
using Microsoft.AspNetCore.SignalR;

namespace IoT_System.Infrastructure.Hubs;

public class IoTHub : Hub
{
    public override async Task OnConnectedAsync()
    {
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
        await Groups.AddToGroupAsync(Context.ConnectionId, Constants.SignalR.Groups.DeviceGroup(deviceId));
    }

    public async Task UnsubscribeFromDevice(Guid deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Constants.SignalR.Groups.DeviceGroup(deviceId));
    }
}