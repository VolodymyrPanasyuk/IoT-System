using IoT_System.Domain.Abstractions;
using IoT_System.Domain.Entities.Enums;

namespace IoT_System.Domain.Entities;

/// <summary>
/// Represents mapping configuration for MeasurementDate field
/// If no mapping exists or parsing fails, CreatedOn is used for MeasurementDate
/// </summary>
public class MeasurementDateMapping : AuditableEntity
{
    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public DataFormat DataFormat { get; set; }

    // Source field path using dot notation
    // JSON example: "metadata.timestamp"
    // XML example: "metadata.timestamp"
    // PlainText: null (use transformations on raw string)
    public string? SourceFieldPath { get; set; }

    // Transformation pipeline stored as JSONB array
    // Common transformations for dates:
    // - UnixTimestamp: { "type": "UnixTimestamp", "config": { "unit": "seconds" } }
    // - ToDateTime: { "type": "ToDateTime", "config": { "format": "yyyy-MM-dd HH:mm:ss" } }
    // - Position extraction: { "type": "Substring", "config": { "startIndex": 0, "length": 19 } }
    public string? TransformationPipeline { get; set; }

    public bool IsActive { get; set; } = true;
}