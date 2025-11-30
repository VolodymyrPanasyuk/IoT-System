namespace IoT_System.Application.Models;

public class ThresholdStatus
{
    public Guid FieldId { get; set; }
    public string FieldName { get; set; } = null!;
    public string Status { get; set; } = null!; // Normal, Warning, Critical
    public decimal? Value { get; set; }
    public decimal? ThresholdValue { get; set; }
    public string? Unit { get; set; }
}