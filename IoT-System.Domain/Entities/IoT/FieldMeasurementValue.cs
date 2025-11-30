using IoT_System.Domain.Abstractions;

namespace IoT_System.Domain.Entities.IoT;

/// <summary>
/// Represents a single field value within a measurement
/// ThresholdStatus is calculated on server-side based on Field threshold configuration
/// </summary>
public class FieldMeasurementValue : Entity
{
    public Guid MeasurementId { get; set; }
    public DeviceMeasurement Measurement { get; set; } = null!;

    public Guid FieldId { get; set; }
    public DeviceField Field { get; set; } = null!;

    // Stored as string for flexibility, parsed based on Field.DataType
    public string Value { get; set; } = null!;
}