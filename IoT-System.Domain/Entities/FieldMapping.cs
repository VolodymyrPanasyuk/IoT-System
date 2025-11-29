using IoT_System.Domain.Abstractions;
using IoT_System.Domain.Entities.Enums;

namespace IoT_System.Domain.Entities;

/// <summary>
/// Represents mapping configuration for a field from different data formats
/// Supports transformation pipeline: multiple operations applied sequentially
/// </summary>
public class FieldMapping : AuditableEntity
{
    public Guid FieldId { get; set; }
    public DeviceField Field { get; set; } = null!;

    public DataFormat DataFormat { get; set; }

    // Source field path using dot notation for nested structures
    // JSON example: "sensors.temperature.value"
    // XML example: "sensors.temperature.value" (XPath-like)
    // PlainText: null (entire raw string is the source)
    public string? SourceFieldPath { get; set; }

    // Transformation pipeline stored as JSONB array
    // Each transformation is applied sequentially
    // Example: [
    //   { "type": "Split", "config": { "delimiter": ",", "position": 2 } },
    //   { "type": "HexDecode", "config": { "encoding": "utf8", "outputType": "decimal" } },
    //   { "type": "Divide", "config": { "value": 10 } }
    // ]
    // If null/empty - use direct mapping from SourceFieldPath
    public string? TransformationPipeline { get; set; }

    public bool IsActive { get; set; } = true;
}