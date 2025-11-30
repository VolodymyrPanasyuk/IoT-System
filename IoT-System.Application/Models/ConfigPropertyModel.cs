namespace IoT_System.Application.Models;

public class ConfigPropertyModel
{
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // "string", "integer", "decimal", "boolean", "array"
    public bool Required { get; set; }
    public string? Description { get; set; }
    public object? DefaultValue { get; set; }
    public List<string>? AllowedValues { get; set; }
}