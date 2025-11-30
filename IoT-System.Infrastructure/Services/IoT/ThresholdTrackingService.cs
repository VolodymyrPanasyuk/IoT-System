using System.Collections.Concurrent;
using IoT_System.Application.Common;

namespace IoT_System.Infrastructure.Services.IoT;

public class ThresholdTrackingService
{
    // Key: DeviceId_FieldId, Value: Current threshold status
    private readonly ConcurrentDictionary<string, string> _thresholdStates = new();

    public bool ShouldNotify(Guid deviceId, Guid fieldId, string newStatus)
    {
        var key = $"{deviceId}_{fieldId}";

        // If Normal, always reset state and don't notify
        if (newStatus == Constants.Thresholds.Normal)
        {
            _thresholdStates.TryRemove(key, out _);
            return false;
        }

        // Check if we already notified about this status
        if (_thresholdStates.TryGetValue(key, out var currentStatus) && currentStatus == newStatus)
        {
            return false; // Already notified about this status
        }

        // Update state and notify
        _thresholdStates[key] = newStatus;
        return true;
    }

    public void ResetField(Guid deviceId, Guid fieldId)
    {
        var key = $"{deviceId}_{fieldId}";
        _thresholdStates.TryRemove(key, out _);
    }

    public void ResetDevice(Guid deviceId)
    {
        var keysToRemove = _thresholdStates.Keys.Where(k => k.StartsWith($"{deviceId}_")).ToList();
        foreach (var key in keysToRemove)
        {
            _thresholdStates.TryRemove(key, out _);
        }
    }
}