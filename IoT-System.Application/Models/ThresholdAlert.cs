namespace IoT_System.Application.Models;

public class ThresholdAlert
{
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = null!;
    public Guid MeasurementId { get; set; }
    public DateTime MeasurementDate { get; set; }
    public List<ThresholdStatus> ExceededThresholds { get; set; } = new();
}