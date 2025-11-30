using IoT_System.Domain.Abstractions;
using IoT_System.Domain.Entities.IoT.Enums;

namespace IoT_System.Domain.Entities.IoT;

/// <summary>
/// Represents a single measurement from device (one data transmission)
/// </summary>
public class DeviceMeasurement : Entity
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    // Raw data for backup/debugging
    public string RawData { get; set; } = null!;

    // Data format used for this measurement
    public DataFormat DataFormat { get; set; }

    // Timestamps
    public DateTime CreatedOn { get; set; }
    public DateTime MeasurementDate { get; set; }

    // Parsing status
    public bool ParsedSuccessfully { get; set; } = true;
    public string? ParsingErrors { get; set; }

    // Navigation properties
    public ICollection<FieldMeasurementValue> FieldValues { get; set; } = new List<FieldMeasurementValue>();
}