using IoT_System.Domain.Abstractions;
using IoT_System.Domain.Entities.Enums;

namespace IoT_System.Domain.Entities;

/// <summary>
/// Represents a measurable field/parameter from device
/// </summary>
public class DeviceField : AuditableEntity
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    // Field information
    public string FieldName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Description { get; set; }
    public FieldDataType DataType { get; set; } = FieldDataType.Decimal;
    public string? Unit { get; set; }

    // Display configuration
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Threshold configuration (decimal with floating point support)
    public decimal? WarningMin { get; set; }
    public decimal? WarningMax { get; set; }
    public decimal? CriticalMin { get; set; }
    public decimal? CriticalMax { get; set; }

    // Navigation properties
    public ICollection<FieldMapping> Mappings { get; set; } = new List<FieldMapping>();
    public ICollection<FieldMeasurementValue> MeasurementValues { get; set; } = new List<FieldMeasurementValue>();
}