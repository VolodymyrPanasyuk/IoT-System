namespace IoT_System.Application.Models;

public class TransformationConfigModel
{
    public int Type { get; set; }
    public string TypeName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public List<ConfigPropertyModel> ConfigProperties { get; set; } = new();
}