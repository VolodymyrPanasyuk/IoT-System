using IoT_System.Domain.Abstractions;

namespace IoT_System.Domain.Entities;

/// <summary>
/// Represents an IoT device that sends measurement data
/// </summary>
public class Device : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Location (coordinates)
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    // API Authentication
    public string ApiKey { get; set; } = null!;
    public DateTime? ApiKeyExpiresAt { get; set; }

    // Navigation properties
    public ICollection<DeviceField> Fields { get; set; } = new List<DeviceField>();
    public ICollection<DeviceMeasurement> Measurements { get; set; } = new List<DeviceMeasurement>();
    public ICollection<DeviceAccessPermission> AccessPermissions { get; set; } = new List<DeviceAccessPermission>();
}